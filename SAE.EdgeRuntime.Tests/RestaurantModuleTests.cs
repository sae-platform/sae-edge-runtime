using SAE.EdgeRuntime.Modules.Restaurant.Aggregates;
using SAE.EdgeRuntime.Modules.Restaurant.Commands;
using SAE.EdgeRuntime.Modules.Restaurant.Events;
using Xunit;

namespace SAE.EdgeRuntime.Tests;

public class RestaurantModuleTests
{
    [Fact]
    public void Table_ShouldBeOccupied_WhenOrderAssigned()
    {
        // Arrange
        var tableId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        
        var command = new OccupyTableCommand(
            Guid.NewGuid(), tableId, tenantId, Guid.NewGuid(), orderId, "Table 5"
        );
        var aggregate = new TableAggregate();

        // Act
        var events = aggregate.Handle(command);

        // Assert
        Assert.Single(events);
        var evt = Assert.IsType<TableOccupiedEvent>(events.First());
        Assert.Equal(orderId, aggregate.CurrentOrderId);
        Assert.True(aggregate.IsOccupied);
    }
}
