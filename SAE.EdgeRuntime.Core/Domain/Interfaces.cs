namespace SAE.EdgeRuntime.Core.Domain;

public interface ICommand
{
    Guid CommandId { get; }
    Guid AggregateId { get; }
    Guid TenantId { get; }
    Guid CorrelationId { get; }
}

public interface IEvent
{
    Guid EventId { get; }
    Guid AggregateId { get; }
    Guid TenantId { get; }
    DateTime OccurredAt { get; }
    int Version { get; }
    
    Guid CorrelationId { get; }
    Guid CausationId { get; }
}
