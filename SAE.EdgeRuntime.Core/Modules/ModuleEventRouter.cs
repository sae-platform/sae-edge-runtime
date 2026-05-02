using System.Collections.Generic;
using System.Threading.Tasks;
using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Core.Modules;

public class ModuleEventRouter
{
    private readonly IEnumerable<IEdgeModule> _modules;
    private readonly ModuleGuard _guard;

    public ModuleEventRouter(IEnumerable<IEdgeModule> modules, ModuleGuard guard)
    {
        _modules = modules;
        _guard = guard;
    }

    public async Task Route(IEvent evt)
    {
        foreach (var module in _modules)
        {
            if (module.CanHandle(evt))
            {
                _guard.Validate(module, evt);
                await Handle(module, evt);
            }
        }
    }

    private async Task Handle(IEdgeModule module, IEvent evt)
    {
        // Execution isolated by module.
        // The router delegates to the module's implementation.
        // Rule: modules do not call each other directly.
        await module.Handle(evt);
    }
}
