using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAE.EdgeRuntime.Core.MultiTenancy;

namespace SAE.EdgeRuntime.Core.Kernel;

public sealed class TenantRuntimeManager(
    IServiceProvider serviceProvider,
    ILogger<TenantRuntimeManager> logger)
{
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _activeTenants = new();
    private readonly IServiceProvider _sp = serviceProvider;
    private readonly ILogger<TenantRuntimeManager> _logger = logger;

    public void StartTenantRuntime(Guid tenantId)
    {
        if (_activeTenants.ContainsKey(tenantId)) return;

        var cts = new CancellationTokenSource();
        if (_activeTenants.TryAdd(tenantId, cts))
        {
            _logger.LogInformation("Initializing runtime for tenant {TenantId}", tenantId);
            
            // Run in background
            _ = Task.Run(async () =>
            {
                using var scope = _sp.CreateScope();
                var tenantContext = new TenantContext(tenantId);
                
                // Get dispatcher loop from DI or create it
                // For simplicity in the skeleton, we create it
                var dispatcher = scope.ServiceProvider.GetRequiredService<TenantDispatcherLoopFactory>()
                    .Create(tenantContext);

                await dispatcher.StartAsync(cts.Token);
            }, cts.Token);
        }
    }

    public void StopTenantRuntime(Guid tenantId)
    {
        if (_activeTenants.TryRemove(tenantId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            _logger.LogInformation("Runtime for tenant {TenantId} stopped", tenantId);
        }
    }
}

// Simple factory to handle dependency injection for dispatcher loops
public sealed class TenantDispatcherLoopFactory(IServiceProvider sp)
{
    public TenantDispatcherLoop Create(TenantContext tenant)
    {
        return ActivatorUtilities.CreateInstance<TenantDispatcherLoop>(sp, tenant);
    }
}
