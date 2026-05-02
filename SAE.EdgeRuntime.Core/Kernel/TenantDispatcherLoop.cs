using Microsoft.Extensions.Logging;
using SAE.EdgeRuntime.Core.Abstractions;
using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;
using SAE.EdgeRuntime.Core.Projections;

namespace SAE.EdgeRuntime.Core.Kernel;

public sealed class TenantDispatcherLoop(
    TenantContext tenant,
    TenantChannelRegistry registry,
    ProjectionEngine projections,
    ILogger<TenantDispatcherLoop> logger)
{
    private readonly TenantContext _tenant = tenant;
    private readonly TenantChannelRegistry _registry = registry;
    private readonly ProjectionEngine _projections = projections;
    private readonly ILogger<TenantDispatcherLoop> _logger = logger;

    public async Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting dispatcher loop for tenant {TenantId}", _tenant.TenantId);
        
        var channel = _registry.GetChannel(_tenant.TenantId);

        try
        {
            await foreach (var @event in channel.Reader.ReadAllAsync(ct))
            {
                _logger.LogDebug("Dispatching event {EventId} for tenant {TenantId}", @event.EventId, _tenant.TenantId);
                
                // 1. Update Read Models
                await _projections.Dispatch(@event);

                // 2. Notify Modules (Logic-level side effects)
                // In a full implementation, we'd have a list of module handlers here
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Dispatcher loop for tenant {TenantId} stopped", _tenant.TenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in dispatcher loop for tenant {TenantId}", _tenant.TenantId);
        }
    }
}
