using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Modules.Catalog.Events;

public record ItemCreatedEvent(
    Guid EventId,
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredAt,
    int Version,
    Guid CorrelationId,
    Guid CausationId,
    string Name,
    string Sku,
    string Barcode,
    decimal BasePrice,
    decimal TaxRate,
    bool TrackInventory
) : IEvent;

public record ItemPriceUpdatedEvent(
    Guid EventId,
    Guid AggregateId,
    Guid TenantId,
    DateTime OccurredAt,
    int Version,
    Guid CorrelationId,
    Guid CausationId,
    decimal NewPrice
) : IEvent;
