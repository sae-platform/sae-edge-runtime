using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;

namespace SAE.EdgeRuntime.Core.Events;

public interface IDomainEventPublisher
{
    Task Publish(IEvent @event, TenantContext tenant);
}

public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly TenantChannelRegistry _registry;

    public DomainEventPublisher(TenantChannelRegistry registry)
    {
        _registry = registry;
    }

    public async Task Publish(IEvent @event, TenantContext tenant)
    {
        var channel = _registry.GetChannel(tenant.TenantId);
        await channel.Writer.WriteAsync(@event);
    }
}
