using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Core.Modules;

public enum ModuleState
{
    Disabled,
    Loading,
    Active,
    Faulted
}

public class ModuleRuntimeEngine
{
    private readonly ModuleLoader _loader;
    private readonly ModuleEventRouter _router;
    private readonly ILogger<ModuleRuntimeEngine> _logger;

    public ModuleRuntimeEngine(ModuleLoader loader, ModuleEventRouter router, ILogger<ModuleRuntimeEngine> logger)
    {
        _loader = loader;
        _router = router;
        _logger = logger;
    }

    public Task Start()
    {
        foreach (var module in _loader.GetModules())
        {
            _logger.LogInformation("Starting module: {ModuleName}", module.Name);
            // Here we would track the ModuleState (e.g., transition from Loading to Active)
        }
        return Task.CompletedTask;
    }

    public async Task Dispatch(IEvent evt)
    {
        try
        {
            await _router.Route(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error routing event {EventId} to modules", evt.EventId);
            // Handle fault tolerance, transition module to Faulted state if necessary
        }
    }
}
