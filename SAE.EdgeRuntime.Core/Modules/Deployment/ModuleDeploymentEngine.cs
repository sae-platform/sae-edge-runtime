using System;
using System.Threading.Tasks;
using SAE.EdgeRuntime.Core.Modules.Versioning;

namespace SAE.EdgeRuntime.Core.Modules.Deployment;

public enum DeploymentStrategy
{
    AllNodes,
    Canary,
    TenantScoped
}

public class DeploymentTarget
{
    public DeploymentStrategy Strategy { get; set; }
    public double CanaryPercentage { get; set; }
    public Guid? TenantId { get; set; }
}

public class ModuleDeploymentEngine
{
    private readonly ModuleVersionRegistry _registry;
    private readonly ModuleRuntimeEngine _runtime;

    public ModuleDeploymentEngine(ModuleVersionRegistry registry, ModuleRuntimeEngine runtime)
    {
        _registry = registry;
        _runtime = runtime;
    }

    public Task Deploy(string moduleName, string version, DeploymentTarget target)
    {
        var module = _registry.Resolve(moduleName, version);

        switch (target.Strategy)
        {
            case DeploymentStrategy.AllNodes:
                return DeployAll(module);

            case DeploymentStrategy.Canary:
                return DeployCanary(module, target.CanaryPercentage);

            case DeploymentStrategy.TenantScoped:
                if (target.TenantId.HasValue)
                    return DeployTenant(module, target.TenantId.Value);
                break;
        }

        return Task.CompletedTask;
    }

    private Task DeployAll(IVersionedEdgeModule module)
    {
        Console.WriteLine($"[Deployment] Deploying {module.Name} v{module.Version} to ALL nodes.");
        // Logic to activate this version globally across the runtime
        return Task.CompletedTask;
    }

    private Task DeployCanary(IVersionedEdgeModule module, double percentage)
    {
        Console.WriteLine($"[Deployment] Deploying {module.Name} v{module.Version} as CANARY ({percentage}%).");
        // Logic to randomly assign requests or tenants based on percentage
        return Task.CompletedTask;
    }

    private Task DeployTenant(IVersionedEdgeModule module, Guid tenantId)
    {
        Console.WriteLine($"[Deployment] Deploying {module.Name} v{module.Version} to TENANT {tenantId}.");
        // Logic to pin this version to a specific tenant execution context
        return Task.CompletedTask;
    }
}
