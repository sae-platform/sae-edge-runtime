using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;

namespace SAE.EdgeRuntime.Core.Storage;

public interface ISnapshotStore
{
    Task SaveAsync(TenantContext tenant, Guid aggregateId, AggregateRoot aggregate);
    Task<(int Version, T Aggregate)?> LoadAsync<T>(TenantContext tenant, Guid aggregateId) where T : AggregateRoot, new();
}

public sealed class SnapshotEngine(ISnapshotStore store)
{
    private readonly ISnapshotStore _store = store;
    private const int SnapshotInterval = 100; // Save snapshot every 100 events

    public bool ShouldSnapshot(AggregateRoot aggregate)
    {
        return aggregate.Version > 0 && aggregate.Version % SnapshotInterval == 0;
    }

    public async Task SaveSnapshotAsync(TenantContext tenant, AggregateRoot aggregate)
    {
        await _store.SaveAsync(tenant, aggregate.Id, aggregate);
    }
}
