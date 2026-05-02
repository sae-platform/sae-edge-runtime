using System.Collections.Generic;

namespace SAE.EdgeRuntime.Core.Modules.Versioning;

public class ModuleCompatibility
{
    public string MinKernelVersion { get; set; } = "1.0.0";
    public string MaxKernelVersion { get; set; } = "*";
    public List<string> SupportedEvents { get; set; } = new();
}

public interface IVersionedEdgeModule : IEdgeModule
{
    string Version { get; }
    ModuleCompatibility Compatibility { get; }
}
