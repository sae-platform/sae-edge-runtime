using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Modules.Orders.Events;

namespace SAE.EdgeRuntime.Modules.Billing.Commands;

public record CreateInvoiceCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid TenantId,
    Guid CorrelationId,
    Guid OrderId,
    List<OrderLineItem> Lines,
    string PaymentMethod
) : ICommand;
