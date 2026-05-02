using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SAE.EdgeRuntime.Core.Modules;

public class ModuleLoader
{
    private readonly List<IEdgeModule> _modules = new();

    public void Load(string assemblyPath, ModuleContext context)
    {
        var assembly = Assembly.LoadFrom(assemblyPath);

        var types = assembly.GetTypes()
            .Where(t => typeof(IEdgeModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in types)
        {
            var module = (IEdgeModule)Activator.CreateInstance(type)!;

            module.Configure(context);
            module.RegisterHandlers(context.EventBus);

            _modules.Add(module);
        }
    }

    public IReadOnlyList<IEdgeModule> GetModules() => _modules;
}
