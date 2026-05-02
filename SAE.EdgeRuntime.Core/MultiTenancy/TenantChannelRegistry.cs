using System.Collections.Concurrent;
using System.Threading.Channels;
using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Core.MultiTenancy;

public class TenantChannelRegistry
{
    private readonly ConcurrentDictionary<Guid, Channel<IEvent>> _channels = new();

    public Channel<IEvent> GetChannel(Guid tenantId)
    {
        return _channels.GetOrAdd(tenantId, _ =>
            Channel.CreateUnbounded<IEvent>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            }));
    }
}
