using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Modules.Caja.Events;

public record CajaSessionOpenedEvent(
    Guid EventId,
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredAt,
    int Version,
    Guid CorrelationId,
    Guid CausationId,
    string TerminalId,
    decimal InitialAmount
) : IEvent;

public record CajaMovementRegisteredEvent(
    Guid EventId,
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredAt,
    int Version,
    Guid CorrelationId,
    Guid CausationId,
    decimal Amount,
    string Type,
    string Reason
) : IEvent;
