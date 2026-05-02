using SAE.EdgeRuntime.Core.Abstractions;
using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.Modules;
using SAE.EdgeRuntime.Modules.Catalog.Events;

namespace SAE.EdgeRuntime.Modules.Catalog;

public class CatalogModule : IEdgeModule
{
    public string Name => "Catalog";

    public void Configure(ModuleContext context)
    {
        // Setup specific dependencies for Catalog
    }

    public void RegisterHandlers(IEventBus bus)
    {
        // Local event registrations if needed
    }

    public bool CanHandle(IEvent evt)
    {
        return evt is ItemCreatedEvent || evt is ItemPriceUpdatedEvent;
    }

    public async Task Handle(IEvent evt)
    {
        // Delegation to specific logic or internal services
        await Task.CompletedTask;
    }
}
