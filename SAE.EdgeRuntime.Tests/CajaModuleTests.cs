using SAE.EdgeRuntime.Modules.Caja.Aggregates;
using SAE.EdgeRuntime.Modules.Caja.Commands;
using SAE.EdgeRuntime.Modules.Caja.Events;
using Xunit;

namespace SAE.EdgeRuntime.Tests;

public class CajaModuleTests
{
    [Fact]
    public void CajaSession_ShouldOpen_WithInitialAmount()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var command = new OpenCajaSessionCommand(
            Guid.NewGuid(), sessionId, tenantId, Guid.NewGuid(), "POS-01", 100m
        );
        var aggregate = new CajaSession();

        // Act
        var events = aggregate.Handle(command);

        // Assert
        Assert.Single(events);
        var evt = Assert.IsType<CajaSessionOpenedEvent>(events.First());
        Assert.Equal(100m, evt.InitialAmount);
        Assert.Equal("POS-01", aggregate.TerminalId);
    }

    [Fact]
    public void CajaSession_ShouldCalculateBalance_WhenCashAdded()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var aggregate = new CajaSession();
        
        aggregate.Handle(new OpenCajaSessionCommand(Guid.NewGuid(), sessionId, tenantId, Guid.NewGuid(), "POS-01", 100m));
        aggregate.MarkCommitted();

        // Act
        aggregate.Handle(new RegisterCajaMovementCommand(Guid.NewGuid(), sessionId, tenantId, Guid.NewGuid(), 50m, "Deposit", "Manual Entry"));

        // Assert
        Assert.Equal(150m, aggregate.CurrentBalance);
    }
}
