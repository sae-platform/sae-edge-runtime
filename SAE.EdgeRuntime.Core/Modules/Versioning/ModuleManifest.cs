using System.Collections.Generic;

namespace SAE.EdgeRuntime.Core.Modules.Versioning;

public class ModuleManifest
{
    public string Name { get; set; }
    public string Version { get; set; }
    
    // Dependencies on other modules (e.g., "Orders" >= "1.0")
    public Dictionary<string, string> Dependencies { get; set; } = new();
}
