using System;
using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Modules.Inventory.Events;

public record StockCorrectionEvent(
    Guid EventId, 
    Guid AggregateId, 
    Guid TenantId, 
    DateTime OccurredAt, 
    int Version, 
    Guid CorrelationId, 
    Guid CausationId,
    string Reason,
    int QuantityAdjusted) : IEvent;
