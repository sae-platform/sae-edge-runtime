using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Modules.Restaurant.Commands;
using SAE.EdgeRuntime.Modules.Restaurant.Events;

namespace SAE.EdgeRuntime.Modules.Restaurant.Aggregates;

public sealed class TableAggregate : AggregateRoot
{
    public Guid? CurrentOrderId { get; private set; }
    public bool IsOccupied => CurrentOrderId.HasValue;
    public string TableName { get; private set; } = string.Empty;

    public IEnumerable<IEvent> Handle(OccupyTableCommand cmd)
    {
        if (IsOccupied) throw new InvalidOperationException("Table is already occupied.");
        
        Raise(new TableOccupiedEvent(
            Guid.NewGuid(), cmd.AggregateId, cmd.TenantId, DateTime.UtcNow, Version + 1,
            cmd.CorrelationId, Guid.NewGuid(), cmd.OrderId, cmd.TableName
        ));
        
        return GetUncommittedEvents();
    }

    public IEnumerable<IEvent> Handle(ReleaseTableCommand cmd)
    {
        if (!IsOccupied) throw new InvalidOperationException("Table is already free.");

        Raise(new TableReleasedEvent(
            Guid.NewGuid(), Id, cmd.TenantId, DateTime.UtcNow, Version + 1,
            cmd.CorrelationId, Guid.NewGuid()
        ));

        return GetUncommittedEvents();
    }

    protected override void Apply(IEvent @event)
    {
        if (@event is TableOccupiedEvent occupied)
        {
            Id = occupied.AggregateId;
            CurrentOrderId = occupied.OrderId;
            TableName = occupied.TableName;
        }
        else if (@event is TableReleasedEvent)
        {
            CurrentOrderId = null;
        }
    }
}
