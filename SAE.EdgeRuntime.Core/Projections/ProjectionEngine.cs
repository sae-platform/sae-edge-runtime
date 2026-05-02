using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;

namespace SAE.EdgeRuntime.Core.Projections;

public interface IProjection
{
    Task HandleAsync(IEvent @event);
}

public sealed class ProjectionEngine(IEnumerable<IProjection> projections)
{
    private readonly IEnumerable<IProjection> _projections = projections;

    public async Task Dispatch(IEvent @event)
    {
        foreach (var projection in _projections)
        {
            await projection.HandleAsync(@event);
        }
    }
}
