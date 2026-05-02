using SAE.EdgeRuntime.Modules.Procurement.Aggregates;
using SAE.EdgeRuntime.Modules.Procurement.Commands;
using SAE.EdgeRuntime.Modules.Procurement.Events;
using Xunit;

namespace SAE.EdgeRuntime.Tests;

public class ProcurementModuleTests
{
    [Fact]
    public void PurchaseOrder_ShouldBeCreated_WithItems()
    {
        // Arrange
        var poId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var items = new List<PurchaseItem>
        {
            new(Guid.NewGuid(), "Meat", 10, 5.50m)
        };

        var command = new CreatePurchaseOrderCommand(
            Guid.NewGuid(), poId, tenantId, Guid.NewGuid(), "SUPP-01", items
        );
        var aggregate = new PurchaseOrderAggregate();

        // Act
        var events = aggregate.Handle(command);

        // Assert
        Assert.Single(events);
        var evt = Assert.IsType<PurchaseOrderCreatedEvent>(events.First());
        Assert.Equal("SUPP-01", aggregate.SupplierId);
        Assert.Equal(55m, aggregate.TotalAmount);
    }
}
