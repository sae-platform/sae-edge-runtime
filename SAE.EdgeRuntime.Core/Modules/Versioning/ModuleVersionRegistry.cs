using System;
using System.Collections.Generic;
using System.Linq;

namespace SAE.EdgeRuntime.Core.Modules.Versioning;

public class ModuleVersionRegistry
{
    private readonly Dictionary<string, List<IVersionedEdgeModule>> _modules = new();

    public void Register(IVersionedEdgeModule module)
    {
        if (!_modules.ContainsKey(module.Name))
            _modules[module.Name] = new List<IVersionedEdgeModule>();

        // Optional: validate if this exact version already exists
        _modules[module.Name].Add(module);
    }

    public IVersionedEdgeModule Resolve(string name, string? version = null)
    {
        if (!_modules.TryGetValue(name, out var versions) || !versions.Any())
            throw new InvalidOperationException($"Module {name} is not registered.");

        if (string.IsNullOrEmpty(version))
            return versions.OrderByDescending(v => v.Version).First();

        var resolved = versions.FirstOrDefault(v => v.Version == version);
        if (resolved == null)
            throw new InvalidOperationException($"Version {version} for module {name} not found.");

        return resolved;
    }

    public IReadOnlyList<IVersionedEdgeModule> GetAllVersions(string name)
    {
        return _modules.TryGetValue(name, out var versions) ? versions : new List<IVersionedEdgeModule>();
    }
}
