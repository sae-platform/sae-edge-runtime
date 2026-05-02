using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.Events;
using SAE.EdgeRuntime.Core.MultiTenancy;
using SAE.EdgeRuntime.Core.Storage;

namespace SAE.EdgeRuntime.Core.Kernel;

public class ExecutionPipeline(
    IDomainEventPublisher publisher,
    IEventStore store,
    IOutboxStore outbox)
{
    private readonly IDomainEventPublisher _publisher = publisher;
    private readonly IEventStore _store = store;
    private readonly IOutboxStore _outbox = outbox;

    public async Task ExecuteAsync(AggregateRoot aggregate, TenantContext tenant)
    {
        var events = aggregate.GetUncommittedEvents().ToList();

        foreach (var evt in events)
        {
            // 1. Persist to Event Store (Source of Truth)
            await _store.AppendAsync(tenant, aggregate.Id, evt);

            // 2. Persist to Outbox (For Sync Fabric)
            await _outbox.StoreAsync(tenant, evt);

            // 3. Publish locally (For immediate module reaction)
            await _publisher.Publish(evt, tenant);
        }

        aggregate.MarkCommitted();
    }
}
