using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;

namespace SAE.EdgeRuntime.Core.Storage;

public class PostgresSnapshotStore : ISnapshotStore
{
    public Task SaveAsync(TenantContext tenant, Guid aggregateId, AggregateRoot aggregate)
    {
        // Conceptual: INSERT INTO snapshot_store (tenant_id, aggregate_id, version, data)
        // ON CONFLICT (tenant_id, aggregate_id) DO UPDATE SET version = EXCLUDED.version, data = EXCLUDED.data
        return Task.CompletedTask;
    }

    public Task<(int Version, T Aggregate)?> LoadAsync<T>(TenantContext tenant, Guid aggregateId) where T : AggregateRoot, new()
    {
        // Conceptual: SELECT version, data FROM snapshot_store WHERE tenant_id = ... AND aggregate_id = ...
        return Task.FromResult<(int, T)?>(null);
    }
}
