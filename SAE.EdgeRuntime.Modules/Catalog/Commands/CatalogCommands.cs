using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Modules.Catalog.Commands;

public record CreateItemCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid TenantId,
    Guid CorrelationId,
    string Name,
    string Sku,
    string Barcode,
    decimal BasePrice,
    decimal TaxRate,
    bool TrackInventory
) : ICommand;

public record UpdateItemPriceCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid TenantId,
    Guid CorrelationId,
    decimal NewPrice
) : ICommand;
