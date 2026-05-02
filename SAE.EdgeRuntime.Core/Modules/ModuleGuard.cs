using System;
using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Core.Modules;

public class ModuleGuard
{
    public void Validate(IEdgeModule module, IEvent evt)
    {
        if (!module.CanHandle(evt))
            throw new InvalidOperationException(
                $"Module {module.Name} cannot handle event {evt.GetType().Name}");
    }
}
