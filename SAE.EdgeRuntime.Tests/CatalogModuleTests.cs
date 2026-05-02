using SAE.EdgeRuntime.Modules.Catalog.Aggregates;
using SAE.EdgeRuntime.Modules.Catalog.Commands;
using SAE.EdgeRuntime.Modules.Catalog.Events;
using Xunit;

namespace SAE.EdgeRuntime.Tests;

public class CatalogModuleTests
{
    [Fact]
    public void Handle_CreateItem_ShouldRaiseItemCreatedEvent()
    {
        // Arrange
        var aggregate = new Item();
        var itemId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var command = new CreateItemCommand(
            Guid.NewGuid(), itemId, tenantId, Guid.NewGuid(),
            "Café Americano", "COF-001", "74400112233", 2.50m, 0.13m, true
        );

        // Act
        var events = aggregate.Handle(command);

        // Assert
        Assert.Single(events);
        var evt = Assert.IsType<ItemCreatedEvent>(events.First());
        Assert.Equal("Café Americano", evt.Name);
        Assert.Equal(2.50m, evt.BasePrice);
        Assert.Equal(itemId, aggregate.Id);
    }

    [Fact]
    public void Handle_UpdatePrice_ShouldRaiseItemPriceUpdatedEvent()
    {
        // Arrange
        var aggregate = new Item();
        var itemId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        
        // Setup initial state via a handle or apply
        aggregate.Handle(new CreateItemCommand(
            Guid.NewGuid(), itemId, tenantId, Guid.NewGuid(),
            "Café Americano", "COF-001", "74400112233", 2.50m, 0.13m, true
        ));
        aggregate.MarkCommitted();

        var updateCommand = new UpdateItemPriceCommand(
            Guid.NewGuid(), itemId, tenantId, Guid.NewGuid(), 3.00m
        );

        // Act
        var events = aggregate.Handle(updateCommand);

        // Assert
        Assert.Single(events);
        var evt = Assert.IsType<ItemPriceUpdatedEvent>(events.First());
        Assert.Equal(3.00m, evt.NewPrice);
        Assert.Equal(3.00m, aggregate.Price);
    }
}
