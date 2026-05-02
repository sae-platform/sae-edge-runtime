using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Modules.Orders.Commands;

public record StartOrderCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid TenantId,
    Guid CorrelationId,
    Guid CustomerId,
    string OrderType
) : ICommand;

public record AddItemToOrderCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid TenantId,
    Guid CorrelationId,
    Guid ItemId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal TaxRate
) : ICommand;

public record CloseOrderCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid TenantId,
    Guid CorrelationId
) : ICommand;
