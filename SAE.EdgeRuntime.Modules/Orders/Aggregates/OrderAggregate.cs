using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Modules.Orders.Commands;
using SAE.EdgeRuntime.Modules.Orders.Events;

namespace SAE.EdgeRuntime.Modules.Orders.Aggregates;

public sealed class OrderAggregate : AggregateRoot
{
    private readonly List<OrderLineItem> _lines = new();
    public bool IsClosed { get; private set; }
    public decimal TotalAmount => _lines.Sum(l => l.UnitPrice * l.Quantity);
    public decimal TotalTax => _lines.Sum(l => (l.UnitPrice * l.Quantity) * l.TaxRate);

    public IEnumerable<IEvent> Handle(StartOrderCommand cmd)
    {
        if (Version > 0) throw new InvalidOperationException("Order already started.");
        
        Raise(new OrderStartedEvent(
            Guid.NewGuid(), cmd.AggregateId, cmd.TenantId, DateTime.UtcNow, 1,
            cmd.CorrelationId, Guid.NewGuid(), cmd.CustomerId, cmd.OrderType
        ));
        
        return GetUncommittedEvents();
    }

    public IEnumerable<IEvent> Handle(AddItemToOrderCommand cmd)
    {
        if (IsClosed) throw new InvalidOperationException("Cannot add items to a closed order.");
        
        var line = new OrderLineItem(cmd.ItemId, cmd.Name, cmd.Quantity, cmd.UnitPrice, cmd.TaxRate);
        
        Raise(new ItemAddedToOrderEvent(
            Guid.NewGuid(), Id, cmd.TenantId, DateTime.UtcNow, Version + 1,
            cmd.CorrelationId, Guid.NewGuid(), line
        ));
        
        return GetUncommittedEvents();
    }

    public IEnumerable<IEvent> Handle(CloseOrderCommand cmd)
    {
        if (IsClosed) throw new InvalidOperationException("Order is already closed.");
        
        Raise(new OrderClosedEvent(
            Guid.NewGuid(), Id, cmd.TenantId, DateTime.UtcNow, Version + 1,
            cmd.CorrelationId, Guid.NewGuid()
        ));
        
        return GetUncommittedEvents();
    }

    protected override void Apply(IEvent @event)
    {
        if (@event is OrderStartedEvent started)
        {
            Id = started.AggregateId;
            IsClosed = false;
        }
        else if (@event is ItemAddedToOrderEvent itemAdded)
        {
            _lines.Add(itemAdded.Item);
        }
        else if (@event is OrderClosedEvent)
        {
            IsClosed = true;
        }
    }
}
