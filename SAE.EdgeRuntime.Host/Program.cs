using Microsoft.EntityFrameworkCore;
using NATS.Net;
using NATS.Client.Core;
using NATS.Client.Hosting;
using SAE.EdgeRuntime.Core;
using SAE.EdgeRuntime.Persistence;
using SAE.EdgeRuntime.Sync;
using Marten;
using SAE.EdgeRuntime.Core.Kernel;
using SAE.EdgeRuntime.Modules.Orders.Commands;
using Weasel.Core;
using SAE.EdgeRuntime.Core.MultiTenancy;
using SAE.EdgeRuntime.Core.Projections;
using SAE.EdgeRuntime.Modules.Orders.Projections;
using SAE.EdgeRuntime.Modules.Catalog.Projections;
using SAE.EdgeRuntime.Modules.Caja.Projections;
using Scalar.AspNetCore;
using SAE.EdgeRuntime.Host.Hubs;
using SAE.EdgeRuntime.Host.Services;
using SAE.EdgeRuntime.Modules.Hardware;
using SAE.EdgeRuntime.Modules.Hardware.Abstractions;
using SAE.EdgeRuntime.Modules.Hardware.Drivers;
using SAE.EdgeRuntime.Modules.Hardware.Renderers;
using SAE.EdgeRuntime.Modules.Hardware.Transports;
using SAE.EdgeRuntime.Modules.Hardware.Discovery;
using SAE.EdgeRuntime.Modules.Hardware.Routing;
using SAE.EdgeRuntime.Modules.Hardware.Observability;
using SAE.EdgeRuntime.Modules.Hardware.Persistence;
using SAE.EdgeRuntime.Modules.Fiscal;
using SAE.EdgeRuntime.Modules.Fiscal.Persistence;
using SAE.EdgeRuntime.Modules.Fiscal.Adapters;
using SAE.Contracts.Hardware;
using SAE.EdgeRuntime.Host.Gateway;
using SAE.Cloud.Edge.Sdk;
using SAE.Cloud.Edge.Sdk.Auth;
using Yarp.ReverseProxy.Transforms;
using System.Threading.RateLimiting;
using System.Net;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

// SAE Edge public key (from cloud config)
var cloudPublicKey = builder.Configuration["Cloud:PublicKey"];
if (!string.IsNullOrEmpty(cloudPublicKey))
{
    builder.Services.AddEdgeAuth(cloudPublicKey);
}

// SAE.Cloud.Edge.Sdk - HTTP client + offline queue
builder.Services.AddEdgeSdk(config =>
{
    config.CloudUrl = builder.Configuration["Cloud:ApiUrl"] ?? "https://localhost:5001";
    config.IdentityUrl = builder.Configuration["Cloud:IdentityUrl"] ?? "https://localhost:5002";
    config.DeviceId = builder.Configuration["EdgeId"] ?? $"edge-{Environment.MachineName}";
    config.DeviceName = builder.Configuration["EdgeName"] ?? Environment.MachineName;
    config.TenantId = builder.Configuration["TenantId"] ?? "";
    config.GracePeriodHours = 72;
});

// ==========================================
// 🚀 EDGE GATEWAY
// ==========================================

// Rate Limiting (per device + per tenant-device)
builder.Services.AddEdgeRateLimiting();

// Memory Cache (GET endpoint responses)
builder.Services.AddMemoryCache();

// Polly HTTP resilience for cloud calls
builder.Services.AddHttpClient("cloud")
    .AddPolicyHandler(EdgeGatewayPolicies.GetCombined());

// YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builder =>
    {
        builder.AddRequestTransform(ctx =>
        {
            var tenantId = ctx.HttpContext.Items["TenantId"]?.ToString();
            if (tenantId != null)
                ctx.ProxyRequest.Headers.Add("X-Tenant-Id", tenantId);
            var deviceId = ctx.HttpContext.Request.Headers["X-Device-Id"].FirstOrDefault();
            if (deviceId != null)
                ctx.ProxyRequest.Headers.Add("X-Device-Id", deviceId);
            return ValueTask.CompletedTask;
        });
    });

// 1. Core Kernel Configuration
builder.Services.AddEdgeKernel();

// 2. Persistence Layer (SAE Elite Hybrid)
var pgConn = builder.Configuration.GetConnectionString("EdgeDb") 
    ?? "Host=localhost;Database=sae_edge;Username=postgres;Password=postgres";

// A. Marten (Domain Events & Business Core)
builder.Services.AddMarten(options => {
    options.Connection(pgConn);
    // options.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.All; 
    options.DatabaseSchemaName = "sae_edge";
    
    // Indexing for Catalog
    options.Schema.For<SAE.EdgeRuntime.Modules.Catalog.Domain.Category>().Index(x => x.Id);
    options.Schema.For<SAE.EdgeRuntime.Modules.Catalog.Domain.Product>().Index(x => x.Id).Index(x => x.CategoryId);
    
    // Indexing for Fiscal
    options.Schema.For<FiscalDocumentDoc>().Index(x => x.ExternalId);
});

// B. EF Core (Master Data / Admin)
builder.Services.AddDbContext<HardwareDbContext>(options => options.UseNpgsql(pgConn));
builder.Services.AddSingleton<LogicalPrinterRepository>();

// C. Dapper + SQLite (Resilient Hot Paths)
builder.Services.AddSingleton<PrintQueueRepository>(sp => new PrintQueueRepository("edge_printing.db"));

builder.Services.AddSingleton<PairingEngine>();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --- Hardware & Printing ---
builder.Services.AddSignalR();
builder.Services.AddSingleton<IPrintNotificationService, SignalRNotificationService>();

// --- Catalog & Identity ---
builder.Services.AddDbContext<SAE.EdgeRuntime.Modules.Identity.Persistence.IdentityDbContext>(options =>
    options.UseNpgsql(pgConn));
builder.Services.AddSingleton<SAE.EdgeRuntime.Modules.Catalog.Persistence.CatalogRepository>();
builder.Services.AddScoped<SAE.EdgeRuntime.Modules.Identity.Persistence.StaffRepository>();
builder.Services.AddHostedService<SAE.EdgeRuntime.Modules.Catalog.Sync.CatalogSyncWorker>();

// Fiscal Services (Marten + Postgres Sequences)
builder.Services.AddSingleton<FiscalSequenceManager>(sp => new FiscalSequenceManager(pgConn));
builder.Services.AddSingleton<FiscalDocumentRepository>();
builder.Services.AddSingleton<IFiscalAdapter, CostaRicaFiscalAdapter>();
builder.Services.AddSingleton<IFiscalAdapter, SaeCloudFiscalAdapter>();
builder.Services.AddSingleton<FiscalOrchestrator>();
builder.Services.AddSingleton<FiscalModule>();

builder.Services.AddSingleton<HardwareModule>();
builder.Services.AddSingleton<PrintWorker>();
builder.Services.AddSingleton<IPrintRenderer, XmlRenderer>();
builder.Services.AddSingleton<IEnumerable<IPrinterDriver>>(sp => new List<IPrinterDriver> 
{ 
    new EscPosDriver(), 
    new ZplDriver() 
});
builder.Services.AddSingleton<IEnumerable<IPrinterTransport>>(sp => new List<IPrinterTransport> 
{ 
    new TcpTransport() 
});

// --- Discovery & Pairing ---
var edgeInfo = new EdgeInfo
{
    EdgeId = builder.Configuration["EdgeId"] ?? "edge-001",
    TenantId = builder.Configuration["TenantId"] ?? "tenant-001",
    Name = builder.Configuration["EdgeName"] ?? Environment.MachineName,
    Ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "127.0.0.1"
};

builder.Services.AddSingleton(edgeInfo);
builder.Services.AddSingleton<UdpBroadcaster>();
builder.Services.AddSingleton<UdpDiscoveryBridge>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar", options => options
        .WithTheme(ScalarTheme.DeepSpace)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
}

app.UseHttpsRedirection();
app.UseCors();

// Edge Auth middleware (validates JWT RS256 + license + device binding)
if (!string.IsNullOrEmpty(cloudPublicKey))
    app.UseEdgeAuth();

// ==========================================
// 🚀 EDGE GATEWAY PIPELINE
// ==========================================
app.UseRateLimiter();                              // 1. Rate limit
app.UseMiddleware<EdgeCacheMiddleware>();          // 2. Cache GET responses
app.UseMiddleware<EdgeCircuitBreakerMiddleware>(); // 3. Circuit breaker
app.MapReverseProxy();                             // 4. Route to Local Backend or Cloud

// API Endpoints for testing the Kernel
app.MapPost("/commands/orders", async (StartOrderCommand cmd, CommandDispatcher dispatcher) =>
{
    // Simulate tenant from header or auth token
    var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    var tenantContext = new TenantContext(tenantId);
    
    await dispatcher.Dispatch(cmd, tenantContext);
    
    return Results.Accepted();
});

app.MapGet("/health", () => Results.Ok(new { Status = "SAE Edge Runtime Online", EdgeId = builder.Configuration["EdgeId"] ?? "edge-001" }));

// Edge Auth - Bootstrap: login to cloud and get edge token
app.MapPost("/api/edge/bootstrap", async (EdgeConfig config, CancellationToken ct) =>
{
    using var client = new HttpClient { BaseAddress = new Uri(config.IdentityUrl) };
    var authClient = new EdgeAuthClient(client, config);
    var token = await authClient.AuthenticateAsync(ct);
    if (token == null)
        return Results.Problem("Cloud authentication failed. Check Cloud:ApiUrl and Cloud:IdentityUrl config.");

    return Results.Ok(new
    {
        status = "authenticated",
        expires_at = token.ExpiresAt,
        tenant_id = token.TenantId,
        offline_grace = token.OfflineGraceHours
    });
});

// Edge Auth - Login for local devices/terminals
app.MapPost("/api/edge/login", async (HttpContext ctx, LocalTokenValidator validator, LicenseValidator licenseValidator, 
    DeviceBindingService deviceBinding, EdgeSessionStore sessionStore) =>
{
    var token = ctx.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
    if (string.IsNullOrEmpty(token))
        return Results.Json(new { error = "missing_token" }, statusCode: 401);

    var payload = validator.Validate(token);
    if (payload == null)
        return Results.Json(new { error = "invalid_token" }, statusCode: 401);

    if (!deviceBinding.Validate(payload.DeviceId))
        return Results.Json(new { error = "device_mismatch" }, statusCode: 403);

    if (!licenseValidator.Validate(payload))
        return Results.Json(new { error = "license_expired" }, statusCode: 402);

    var session = sessionStore.Create(payload);
    return Results.Ok(new
    {
        session_id = session.SessionId,
        tenant_id = payload.TenantId,
        modules = payload.Modules,
        permissions = payload.Permissions,
        license_until = payload.LicenseValidUntil
    });
});

// Edge Info - current device status
app.MapGet("/api/edge/info", (EdgeConfig config, DeviceBindingService deviceBinding) => Results.Ok(new
{
    device_id = deviceBinding.GetDeviceId(),
    config_id = config.DeviceId,
    tenant_id = config.TenantId,
    cloud_url = config.CloudUrl
}));

app.MapPost("/api/hardware/print", async (PrintApiRequest req, HardwareModule hardware) =>
{
    var jobId = Guid.NewGuid().ToString();
    await hardware.PrintAsync(jobId, req.Target, req.TemplateId, req.Data);
    return Results.Ok(new { jobId });
});

app.MapGet("/api/hardware/metrics", (MetricsStore metrics) => Results.Ok(metrics.GetSnapshot()));

app.MapHub<PrintHub>("/ws/print");

// Start Background Services
var worker = app.Services.GetRequiredService<PrintWorker>();
_ = Task.Run(() => worker.StartAsync(CancellationToken.None));

var broadcaster = app.Services.GetRequiredService<UdpBroadcaster>();
_ = Task.Run(() => broadcaster.StartAsync(CancellationToken.None));

var bridge = app.Services.GetRequiredService<UdpDiscoveryBridge>();
_ = Task.Run(() => bridge.StartListeningAsync(CancellationToken.None));

app.Run();
