using Microsoft.Extensions.Logging;
using SAE.EdgeRuntime.Core.Abstractions;
using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.Modules;
using SAE.EdgeRuntime.Modules.Inventory.Events;
using SAE.EdgeRuntime.Modules.Inventory.Services;
using SAE.EdgeRuntime.Modules.Inventory.Domain;
using SAE.EdgeRuntime.Modules.Orders.Events;

namespace SAE.EdgeRuntime.Modules.Inventory;

public class InventoryModule : IEdgeModule
{
    public string Name => "Inventory";
    private readonly RecipeManager _recipeManager = new();

    public void Configure(ModuleContext context)
    {
        // Initialize state, dependencies
    }

    public void RegisterHandlers(IEventBus bus)
    {
        // bus.Subscribe<OrderClosedEvent>(Handle);
    }

    public bool CanHandle(IEvent evt)
    {
        return evt is OrderClosedEvent || evt is StockCorrectionEvent || evt is ItemAddedToOrderEvent;
    }

    public async Task Handle(IEvent evt)
    {
        if (evt is ItemAddedToOrderEvent itemSold)
        {
            var movements = _recipeManager.Decompose(itemSold.Item.ItemId, itemSold.Item.Quantity);
            foreach (var move in movements)
            {
                Console.WriteLine($"[Inventory] STOCK DEDUCTION: Item {move.ItemId} Qty {move.Quantity} (Type: {move.Type})");
            }
        }
        else if (evt is OrderClosedEvent closedEvt)
        {
            await HandleInternal(closedEvt);
        }
    }

    private Task HandleInternal(OrderClosedEvent evt)
    {
        Console.WriteLine($"[InventoryModule] Order closed handled. Executing L3 Reconciliation logic for Order: {evt.AggregateId}");
        
        // L3 reconciliation logic (Semantic Rule: No Negative Stock)
        
        // 1. Reconstruct current stock from local Event Store
        // int currentStock = await _store.GetStock(productId);
        int currentStock = 5; // conceptual
        int requestedDeduction = 10; // conceptual from OrderClosedEvent
 
        if (currentStock - requestedDeduction < 0)
        {
            Console.WriteLine($"[L3 Conflict] SPLIT-BRAIN DETECTED: Stock would drop below zero. Executing Convergence Algorithm.");
            
            // Generate a compensating event instead of hard-failing the sync
            var correctionEvent = new Events.StockCorrectionEvent(
                Guid.NewGuid(), evt.AggregateId, evt.TenantId, DateTime.UtcNow, evt.Version + 1, evt.CorrelationId, Guid.NewGuid(),
                "Prevented Negative Stock", requestedDeduction - currentStock);
            
            Console.WriteLine($"[L3 Resolution] Generated StockCorrectionEvent to rebalance stock by +{correctionEvent.QuantityAdjusted}");
            
            // Publish correction event to the local bus / outbox
        }
        else
        {
            Console.WriteLine("[L3 OK] Stock deduction successful. No conflict.");
        }
        
        return Task.CompletedTask;
    }

    // For TDD/Demo purposes
    public void RegisterRecipe(Recipe recipe) => _recipeManager.RegisterRecipe(recipe);
}
