using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Modules.Restaurant.Events;

public record TableOccupiedEvent(
    Guid EventId,
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredAt,
    int Version,
    Guid CorrelationId,
    Guid CausationId,
    Guid OrderId,
    string TableName
) : IEvent;

public record TableReleasedEvent(
    Guid EventId,
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredAt,
    int Version,
    Guid CorrelationId,
    Guid CausationId
) : IEvent;
