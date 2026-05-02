using System;
using System.Collections.Generic;
using System.Linq;

namespace SAE.EdgeRuntime.Core.Modules.Versioning;

public class DependencyResolver
{
    // Simple topological sort (DAG processor) for module initialization/replay order
    public List<ModuleManifest> ResolveOrder(IEnumerable<ModuleManifest> manifests)
    {
        var sorted = new List<ModuleManifest>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>(); // To detect circular dependencies

        var manifestDict = manifests.ToDictionary(m => m.Name);

        void Visit(ModuleManifest node)
        {
            if (visited.Contains(node.Name)) return;
            
            if (visiting.Contains(node.Name))
                throw new InvalidOperationException($"Circular dependency detected involving module {node.Name}");

            visiting.Add(node.Name);

            foreach (var dep in node.Dependencies.Keys)
            {
                if (manifestDict.TryGetValue(dep, out var dependencyNode))
                {
                    Visit(dependencyNode);
                }
                else
                {
                    throw new InvalidOperationException($"Missing dependency: {node.Name} requires {dep}");
                }
            }

            visiting.Remove(node.Name);
            visited.Add(node.Name);
            sorted.Add(node);
        }

        foreach (var manifest in manifests)
        {
            Visit(manifest);
        }

        return sorted;
    }
}
