using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Modules.Orders.Events;

namespace SAE.EdgeRuntime.Modules.Billing.Events;

public record InvoiceGeneratedEvent(
    Guid EventId,
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredAt,
    int Version,
    Guid CorrelationId,
    Guid CausationId,
    Guid OrderId,
    string FiscalNumber,
    decimal SubTotal,
    decimal TotalTax,
    decimal TotalWithTax,
    string PaymentMethod
) : IEvent;
