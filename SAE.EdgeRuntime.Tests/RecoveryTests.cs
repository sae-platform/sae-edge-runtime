using Moq;
using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;
using SAE.EdgeRuntime.Core.Recovery;
using SAE.EdgeRuntime.Core.Storage;
using SAE.EdgeRuntime.Modules.Orders.Aggregates;
using SAE.EdgeRuntime.Modules.Orders.Events;
using Xunit;

namespace SAE.EdgeRuntime.Tests;

public class RecoveryTests
{
    private readonly Mock<IEventStore> _eventStoreMock;
    private readonly Mock<ISnapshotStore> _snapshotStoreMock;
    private readonly RecoveryEngine _recoveryEngine;
    private readonly TenantContext _tenant;

    public RecoveryTests()
    {
        _eventStoreMock = new Mock<IEventStore>();
        _snapshotStoreMock = new Mock<ISnapshotStore>();
        _recoveryEngine = new RecoveryEngine(_eventStoreMock.Object, _snapshotStoreMock.Object);
        _tenant = new TenantContext(Guid.NewGuid());
    }

    [Fact]
    public async Task RebuildAsync_ShouldRestoreAggregateState_FromEvents()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var events = new List<IEvent>
        {
            new OrderStartedEvent(Guid.NewGuid(), aggregateId, _tenant.TenantId, DateTime.UtcNow, 1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Table"),
            new ItemAddedToOrderEvent(Guid.NewGuid(), aggregateId, _tenant.TenantId, DateTime.UtcNow, 2, Guid.NewGuid(), Guid.NewGuid(), new OrderLineItem(Guid.NewGuid(), "Coffee", 1, 5m, 0m))
        };

        _snapshotStoreMock.Setup(s => s.LoadAsync<OrderAggregate>(_tenant, aggregateId))
            .ReturnsAsync(((int Version, OrderAggregate Aggregate)?)null);

        _eventStoreMock.Setup(s => s.LoadEvents(_tenant, aggregateId))
            .ReturnsAsync(events);

        // Act
        var aggregate = await _recoveryEngine.RebuildAsync<OrderAggregate>(aggregateId, _tenant);

        // Assert
        Assert.NotNull(aggregate);
        Assert.Equal(aggregateId, aggregate.Id);
        Assert.Equal(2, aggregate.Version);
        Assert.Equal(5m, aggregate.TotalAmount);
    }
}
