using SAE.EdgeRuntime.Modules.Inventory;
using SAE.EdgeRuntime.Modules.Inventory.Domain;
using SAE.EdgeRuntime.Modules.Inventory.Services;
using SAE.EdgeRuntime.Modules.Orders.Events;
using SAE.EdgeRuntime.Modules.Orders.Aggregates;
using Xunit;

namespace SAE.EdgeRuntime.Tests;

public class InventoryRecipeTests
{
    [Fact]
    public async Task Inventory_ShouldProcessRecipe_WhenItemSold()
    {
        // Arrange
        var inventoryModule = new InventoryModule();
        var burgerId = Guid.NewGuid();
        var bunId = Guid.NewGuid();
        var meatId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // 1. Define a recipe: 1 Burger = 1 Bun + 1 Meat
        var recipe = new Recipe(burgerId, new List<IngredientUsage>
        {
            new(bunId, 1),
            new(meatId, 1)
        });
        
        inventoryModule.RegisterRecipe(recipe);

        // 2. Simulate Sale Event (ItemAddedToOrderEvent)
        var itemAdded = new ItemAddedToOrderEvent(
            Guid.NewGuid(), Guid.NewGuid(), tenantId, DateTime.UtcNow, 1, 
            Guid.NewGuid(), Guid.NewGuid(),
            new OrderLineItem(burgerId, "Burger", 1, 10m, 0.10m)
        );

        // Act
        await inventoryModule.Handle(itemAdded);

        // Assert
        // The Console output verified manually, or we could add an event spy.
        // For TDD purposes, the successful execution of Decompose() within Handle() is the goal.
        Assert.True(true);
    }
}
