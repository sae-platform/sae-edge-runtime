using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Modules.Procurement.Events;

public record PurchaseItem(Guid ItemId, string Name, decimal Quantity, decimal UnitCost);

public record PurchaseOrderCreatedEvent(
    Guid EventId,
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredAt,
    int Version,
    Guid CorrelationId,
    Guid CausationId,
    string SupplierId,
    List<PurchaseItem> Items,
    decimal TotalAmount
) : IEvent;

public record PurchaseOrderReceivedEvent(
    Guid EventId,
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredAt,
    int Version,
    Guid CorrelationId,
    Guid CausationId
) : IEvent;
