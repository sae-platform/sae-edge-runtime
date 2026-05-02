using SAE.EdgeRuntime.Core.Storage;
using SAE.EdgeRuntime.Fabric.Models;
using SAE.EdgeRuntime.Core.MultiTenancy;

namespace SAE.EdgeRuntime.Fabric;

public sealed class ConflictResolver(IEventStore store)
{
    private readonly IEventStore _store = store;

    public async Task ResolveAndApply(FabricEventEnvelope evt)
    {
        var tenant = new TenantContext(evt.TenantId);
        
        // Get last version from local store
        // Note: We need to add GetLastVersion to IEventStore or handle it here
        // For the skeleton, we'll assume a local version check
        var events = await _store.LoadEvents(tenant, evt.AggregateId);
        var lastVersion = events.Any() ? events.Max(e => 0) : -1; // Conceptual

        // L1 Strategy: Last Write Wins / Version ordering strict
        if (evt.Version == lastVersion + 1)
        {
            await Apply(evt, tenant);
            return;
        }

        if (evt.Version <= lastVersion)
        {
            // Already processed or conflict
            await HandleDuplicateOrConflict(evt);
            return;
        }

        // Gap detected: request missing events
        await RequestMissingEvents(evt.AggregateId);
    }

    private async Task Apply(FabricEventEnvelope evt, TenantContext tenant)
    {
        // 1. Persist to local EventStore
        // 2. Trigger local projections
        // await _store.AppendAsync(tenant, evt.AggregateId, ...);
    }

    private Task HandleDuplicateOrConflict(FabricEventEnvelope evt)
    {
        // Log or handle L3 domain-specific rules
        return Task.CompletedTask;
    }

    private Task RequestMissingEvents(Guid aggregateId)
    {
        // Trigger a fetch from peer or cloud
        return Task.CompletedTask;
    }
}
