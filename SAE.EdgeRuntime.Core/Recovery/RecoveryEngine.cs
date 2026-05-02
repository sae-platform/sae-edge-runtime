using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;
using SAE.EdgeRuntime.Core.Storage;

namespace SAE.EdgeRuntime.Core.Recovery;

public class RecoveryEngine(IEventStore store, ISnapshotStore snapshotStore)
{
    private readonly IEventStore _store = store;
    private readonly ISnapshotStore _snapshotStore = snapshotStore;

    public async Task<T> RebuildAsync<T>(Guid aggregateId, TenantContext tenant)
        where T : AggregateRoot, new()
    {
        // 1. Try loading latest snapshot
        var snapshot = await _snapshotStore.LoadAsync<T>(tenant, aggregateId);
        
        var aggregate = snapshot?.Aggregate ?? new T();
        var startVersion = snapshot?.Version ?? -1;

        // 2. Load events after snapshot version
        var events = await _store.LoadEvents(tenant, aggregateId);
        
        // 3. Replay only delta events
        foreach (var evt in events.Where(e => ((dynamic)e).Version > startVersion))
        {
            aggregate.ApplyFromHistory(evt);
        }

        return aggregate;
    }
}
