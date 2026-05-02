using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Modules.Orders.Events;

public record OrderLineItem(
    Guid ItemId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal TaxRate
);

public record OrderStartedEvent(
    Guid EventId,
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredAt,
    int Version,
    Guid CorrelationId,
    Guid CausationId,
    Guid CustomerId,
    string OrderType // Table, Express, PickUp
) : IEvent;

public record ItemAddedToOrderEvent(
    Guid EventId,
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredAt,
    int Version,
    Guid CorrelationId,
    Guid CausationId,
    OrderLineItem Item
) : IEvent;

public record OrderClosedEvent(
    Guid EventId,
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredAt,
    int Version,
    Guid CorrelationId,
    Guid CausationId
) : IEvent;
