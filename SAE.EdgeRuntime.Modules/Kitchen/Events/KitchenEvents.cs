using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Modules.Kitchen.Events;

public enum KitchenOrderStatus
{
    Pending,
    Preparing,
    Ready,
    Served
}

public record OrderSentToKitchenEvent(
    Guid EventId,
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredAt,
    int Version,
    Guid CorrelationId,
    Guid CausationId,
    Guid OrderId,
    string ItemName,
    int Quantity
) : IEvent;

public record KitchenOrderStartedEvent(
    Guid EventId,
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredAt,
    int Version,
    Guid CorrelationId,
    Guid CausationId
) : IEvent;

public record KitchenOrderReadyEvent(
    Guid EventId,
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredAt,
    int Version,
    Guid CorrelationId,
    Guid CausationId
) : IEvent;
