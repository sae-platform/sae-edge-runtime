using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System.Threading.RateLimiting;

namespace SAE.EdgeRuntime.Host.Gateway;

public static class EdgeGatewayPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions.HandleTransientHttpError()
            .WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(200 * retry));

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
        HttpPolicyExtensions.HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy() =>
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(3));

    public static IAsyncPolicy<HttpResponseMessage> GetCombined() =>
        Policy.WrapAsync(GetTimeoutPolicy(), GetCircuitBreakerPolicy(), GetRetryPolicy());

    public static void AddEdgeRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("per-device", ctx =>
            {
                var deviceId = ctx.Request.Headers["X-Device-Id"].FirstOrDefault() ?? "anon";
                return RateLimitPartition.GetFixedWindowLimiter(deviceId,
                    _ => new FixedWindowRateLimiterOptions { PermitLimit = 100, Window = TimeSpan.FromSeconds(10), QueueLimit = 20 });
            });

            options.AddPolicy("per-tenant-device", ctx =>
            {
                var tenant = ctx.Items["TenantId"]?.ToString() ?? "anon";
                var device = ctx.Request.Headers["X-Device-Id"].FirstOrDefault() ?? "anon";
                return RateLimitPartition.GetSlidingWindowLimiter($"{tenant}:{device}",
                    _ => new SlidingWindowRateLimiterOptions { PermitLimit = 200, Window = TimeSpan.FromSeconds(30), SegmentsPerWindow = 3 });
            });

            options.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.StatusCode = 429;
                await ctx.HttpContext.Response.WriteAsync("{\"error\":\"rate_limit_exceeded\"}", ct);
            };
        });
    }
}

public class EdgeCacheMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    public EdgeCacheMiddleware(RequestDelegate next, IMemoryCache cache) { _next = next; _cache = cache; }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method != HttpMethods.Get)
        {
            await _next(context); return;
        }

        var tenant = context.Items["TenantId"]?.ToString() ?? "default";
        var key = $"edge-cache:{tenant}:{context.Request.Path}{context.Request.QueryString}";

        if (_cache.TryGetValue(key, out string? cached) && cached != null)
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cached);
            return;
        }

        var originalBody = context.Response.Body;
        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        await _next(context);

        if (context.Response.StatusCode == 200)
        {
            memStream.Position = 0;
            var responseBody = await new StreamReader(memStream).ReadToEndAsync();
            _cache.Set(key, responseBody, TimeSpan.FromSeconds(30));
            memStream.Position = 0;
        }

        memStream.Position = 0;
        await memStream.CopyToAsync(originalBody);
    }
}

public class EdgeCircuitBreakerMiddleware
{
    private readonly RequestDelegate _next;
    private static int _failureCount;
    private static DateTime _openUntil = DateTime.MinValue;
    private static readonly object _lock = new();

    public EdgeCircuitBreakerMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (DateTime.UtcNow < _openUntil)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsync("{\"error\":\"circuit_open\"}");
            return;
        }

        try
        {
            await _next(context);
        }
        catch
        {
            lock (_lock)
            {
                _failureCount++;
                if (_failureCount >= 5)
                {
                    _openUntil = DateTime.UtcNow.AddSeconds(30);
                    _failureCount = 0; // reset for next cycle
                }
            }
            context.Response.StatusCode = 502;
            await context.Response.WriteAsync("{\"error\":\"backend_unavailable\"}");
        }
    }
}
