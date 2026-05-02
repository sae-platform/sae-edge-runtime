using System.Threading.Tasks;
using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Core.Abstractions;

public interface IEventBus
{
    Task PublishAsync(IEvent evt);
    void Subscribe<T>(Func<T, Task> handler) where T : IEvent;
}
