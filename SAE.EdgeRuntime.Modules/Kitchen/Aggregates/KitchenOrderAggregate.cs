using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Modules.Kitchen.Events;

namespace SAE.EdgeRuntime.Modules.Kitchen.Aggregates;

public sealed class KitchenOrderAggregate : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public KitchenOrderStatus Status { get; private set; }

    public IEnumerable<IEvent> HandleSendToKitchen(Guid aggregateId, Guid tenantId, Guid orderId, string itemName, int quantity)
    {
        if (Version > 0) throw new InvalidOperationException("Kitchen order already exists.");

        Raise(new OrderSentToKitchenEvent(
            Guid.NewGuid(), aggregateId, tenantId, DateTime.UtcNow, 1,
            Guid.NewGuid(), Guid.NewGuid(), orderId, itemName, quantity
        ));

        return GetUncommittedEvents();
    }

    public IEnumerable<IEvent> HandleStartPreparation(Guid tenantId)
    {
        if (Status != KitchenOrderStatus.Pending) throw new InvalidOperationException("Order must be pending to start preparation.");

        Raise(new KitchenOrderStartedEvent(
            Guid.NewGuid(), Id, tenantId, DateTime.UtcNow, Version + 1,
            Guid.NewGuid(), Guid.NewGuid()
        ));

        return GetUncommittedEvents();
    }

    protected override void Apply(IEvent @event)
    {
        if (@event is OrderSentToKitchenEvent sent)
        {
            Id = sent.AggregateId;
            OrderId = sent.OrderId;
            ItemName = sent.ItemName;
            Quantity = sent.Quantity;
            Status = KitchenOrderStatus.Pending;
        }
        else if (@event is KitchenOrderStartedEvent)
        {
            Status = KitchenOrderStatus.Preparing;
        }
        else if (@event is KitchenOrderReadyEvent)
        {
            Status = KitchenOrderStatus.Ready;
        }
    }
}
