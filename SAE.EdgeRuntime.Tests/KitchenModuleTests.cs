using SAE.EdgeRuntime.Modules.Kitchen.Aggregates;
using SAE.EdgeRuntime.Modules.Kitchen.Events;
using Xunit;

namespace SAE.EdgeRuntime.Tests;

public class KitchenModuleTests
{
    [Fact]
    public void KitchenOrder_ShouldBeCreated_WhenSentToKitchen()
    {
        // Arrange
        var kitchenOrderId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        
        var aggregate = new KitchenOrderAggregate();

        // Act
        // Note: We'll use a direct event application or a command to simulate the process
        var events = aggregate.HandleSendToKitchen(kitchenOrderId, tenantId, orderId, "Burger", 2);

        // Assert
        Assert.Single(events);
        var evt = Assert.IsType<OrderSentToKitchenEvent>(events.First());
        Assert.Equal("Burger", aggregate.ItemName);
        Assert.Equal(KitchenOrderStatus.Pending, aggregate.Status);
    }
}
