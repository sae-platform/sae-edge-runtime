using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;

namespace SAE.EdgeRuntime.Core.Storage;

public class PostgresEventStore : IEventStore
{
    public Task AppendAsync(
        TenantContext tenant,
        Guid aggregateId,
        IEvent @event)
    {
        // Conceptual: INSERT INTO event_store (tenant_id, aggregate_id, event_data...)
        // This will be expanded in the Persistence project implementation
        return Task.CompletedTask;
    }

    public Task<IEnumerable<IEvent>> LoadEvents(TenantContext tenant, Guid aggregateId)
    {
        // Conceptual: SELECT event_data FROM event_store WHERE tenant_id = ...
        return Task.FromResult<IEnumerable<IEvent>>(new List<IEvent>());
    }
}
