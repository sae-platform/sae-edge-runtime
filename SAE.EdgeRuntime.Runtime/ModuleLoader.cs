using Microsoft.Extensions.DependencyInjection;
using SAE.EdgeRuntime.Core.Abstractions;
using SAE.EdgeRuntime.Core.Modules;

namespace SAE.EdgeRuntime.Runtime;

public sealed class ModuleLoader(IEventBus eventBus, IServiceCollection services)
{
    private readonly IEventBus _eventBus = eventBus;
    private readonly IServiceCollection _services = services;

    public void Load(IEdgeModule module)
    {
        module.RegisterHandlers(_eventBus);
    }
}
