using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Modules.Procurement.Events;

namespace SAE.EdgeRuntime.Modules.Procurement.Commands;

public record CreatePurchaseOrderCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid TenantId,
    Guid CorrelationId,
    string SupplierId,
    List<PurchaseItem> Items
) : ICommand;

public record ReceivePurchaseOrderCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid TenantId,
    Guid CorrelationId
) : ICommand;
