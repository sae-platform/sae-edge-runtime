using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Modules.Restaurant.Commands;

public record OccupyTableCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid TenantId,
    Guid CorrelationId,
    Guid OrderId,
    string TableName
) : ICommand;

public record ReleaseTableCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid TenantId,
    Guid CorrelationId
) : ICommand;
