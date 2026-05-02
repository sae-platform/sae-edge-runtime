using Moq;
using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.Events;
using SAE.EdgeRuntime.Core.Kernel;
using SAE.EdgeRuntime.Core.MultiTenancy;
using SAE.EdgeRuntime.Core.Storage;
using SAE.EdgeRuntime.Modules.Orders.Aggregates;
using SAE.EdgeRuntime.Modules.Orders.Commands;
using Xunit;

namespace SAE.EdgeRuntime.Tests;

public class ExecutionPipelineTests
{
    private readonly Mock<IEventStore> _eventStoreMock;
    private readonly Mock<IOutboxStore> _outboxStoreMock;
    private readonly Mock<IDomainEventPublisher> _publisherMock;
    private readonly ExecutionPipeline _pipeline;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly TenantContext _tenant;

    public ExecutionPipelineTests()
    {
        _eventStoreMock = new Mock<IEventStore>();
        _outboxStoreMock = new Mock<IOutboxStore>();
        _publisherMock = new Mock<IDomainEventPublisher>();
        _pipeline = new ExecutionPipeline(_publisherMock.Object, _eventStoreMock.Object, _outboxStoreMock.Object);
        _tenant = new TenantContext(_tenantId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSaveEventsToStoreAndOutbox()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var aggregate = new OrderAggregate();
        var command = new StartOrderCommand(Guid.NewGuid(), orderId, _tenantId, Guid.NewGuid(), Guid.NewGuid(), "Table");
        
        // Generate events by handling a command
        aggregate.Handle(command);
        var eventsCount = aggregate.GetUncommittedEvents().Count();

        // Act
        await _pipeline.ExecuteAsync(aggregate, _tenant);

        // Assert
        _eventStoreMock.Verify(s => s.AppendAsync(_tenant, orderId, It.IsAny<IEvent>()), Times.Exactly(eventsCount));
        _outboxStoreMock.Verify(s => s.StoreAsync(_tenant, It.IsAny<IEvent>()), Times.Exactly(eventsCount));
        _publisherMock.Verify(p => p.Publish(It.IsAny<IEvent>(), _tenant), Times.Exactly(eventsCount));
    }
}

public record TestEvent(Guid EventId, Guid AggregateId, Guid TenantId) : IEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
    public Guid CorrelationId { get; } = Guid.NewGuid();
    public Guid CausationId { get; } = Guid.NewGuid();
}
