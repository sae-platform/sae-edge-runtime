using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Modules.Procurement.Commands;
using SAE.EdgeRuntime.Modules.Procurement.Events;

namespace SAE.EdgeRuntime.Modules.Procurement.Aggregates;

public sealed class PurchaseOrderAggregate : AggregateRoot
{
    public string SupplierId { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public bool IsReceived { get; private set; }

    public IEnumerable<IEvent> Handle(CreatePurchaseOrderCommand cmd)
    {
        if (Version > 0) throw new InvalidOperationException("Purchase order already exists.");

        var total = cmd.Items.Sum(i => i.Quantity * i.UnitCost);

        Raise(new PurchaseOrderCreatedEvent(
            Guid.NewGuid(), cmd.AggregateId, cmd.TenantId, DateTime.UtcNow, 1,
            cmd.CorrelationId, Guid.NewGuid(), cmd.SupplierId, cmd.Items, total
        ));

        return GetUncommittedEvents();
    }

    protected override void Apply(IEvent @event)
    {
        if (@event is PurchaseOrderCreatedEvent created)
        {
            Id = created.AggregateId;
            SupplierId = created.SupplierId;
            TotalAmount = created.TotalAmount;
            IsReceived = false;
        }
        else if (@event is PurchaseOrderReceivedEvent)
        {
            IsReceived = true;
        }
    }
}
