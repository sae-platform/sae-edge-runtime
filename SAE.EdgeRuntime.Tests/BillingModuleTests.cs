using SAE.EdgeRuntime.Modules.Billing.Aggregates;
using SAE.EdgeRuntime.Modules.Billing.Commands;
using SAE.EdgeRuntime.Modules.Billing.Events;
using SAE.EdgeRuntime.Modules.Orders.Events;
using Xunit;

namespace SAE.EdgeRuntime.Tests;

public class BillingModuleTests
{
    [Fact]
    public void Invoice_ShouldGenerate_WhenOrderIsClosed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        
        var lines = new List<OrderLineItem>
        {
            new(Guid.NewGuid(), "Burger", 1, 10m, 0.10m)
        };

        var command = new CreateInvoiceCommand(
            Guid.NewGuid(), invoiceId, tenantId, Guid.NewGuid(), orderId, lines, "Cash"
        );
        var aggregate = new InvoiceAggregate();

        // Act
        var events = aggregate.Handle(command);

        // Assert
        Assert.Single(events);
        var evt = Assert.IsType<InvoiceGeneratedEvent>(events.First());
        Assert.Equal(11m, evt.TotalWithTax);
        Assert.NotEmpty(evt.FiscalNumber);
    }
}
