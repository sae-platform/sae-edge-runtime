using SAE.EdgeRuntime.Modules.Orders.Aggregates;
using SAE.EdgeRuntime.Modules.Orders.Commands;
using SAE.EdgeRuntime.Modules.Orders.Events;
using Xunit;

namespace SAE.EdgeRuntime.Tests;

public class HighFidelityOrderTests
{
    [Fact]
    public void Order_ShouldCalculateCorrectTotals_WhenMultipleItemsAdded()
    {
        // Arrange
        var aggregate = new OrderAggregate();
        var orderId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        
        aggregate.Handle(new StartOrderCommand(Guid.NewGuid(), orderId, tenantId, Guid.NewGuid(), Guid.NewGuid(), "Table"));

        // Act
        aggregate.Handle(new AddItemToOrderCommand(Guid.NewGuid(), orderId, tenantId, Guid.NewGuid(), Guid.NewGuid(), "Burger", 2, 10m, 0.10m)); // 20 + 2 tax
        aggregate.Handle(new AddItemToOrderCommand(Guid.NewGuid(), orderId, tenantId, Guid.NewGuid(), Guid.NewGuid(), "Soda", 1, 5m, 0.05m));   // 5 + 0.25 tax

        // Assert
        Assert.Equal(25m, aggregate.TotalAmount);
        Assert.Equal(2.25m, aggregate.TotalTax);
    }

    [Fact]
    public void Order_ShouldPreventChanges_WhenClosed()
    {
        // Arrange
        var aggregate = new OrderAggregate();
        var orderId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        
        aggregate.Handle(new StartOrderCommand(Guid.NewGuid(), orderId, tenantId, Guid.NewGuid(), Guid.NewGuid(), "Table"));
        aggregate.Handle(new CloseOrderCommand(Guid.NewGuid(), orderId, tenantId, Guid.NewGuid()));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            aggregate.Handle(new AddItemToOrderCommand(Guid.NewGuid(), orderId, tenantId, Guid.NewGuid(), Guid.NewGuid(), "Forbidden", 1, 10m, 0m))
        );
    }
}
