using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;

namespace SAE.EdgeRuntime.Core.Storage;

public interface IEventStore
{
    Task AppendAsync(TenantContext tenant, Guid aggregateId, IEvent @event);
    Task<IEnumerable<IEvent>> LoadEvents(TenantContext tenant, Guid aggregateId);
}
