using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Modules.Caja.Commands;

public record OpenCajaSessionCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid TenantId,
    Guid CorrelationId,
    string TerminalId,
    decimal InitialAmount
) : ICommand;

public record RegisterCajaMovementCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid TenantId,
    Guid CorrelationId,
    decimal Amount,
    string Type,
    string Reason
) : ICommand;
