using SAE.EdgeRuntime.Core.Abstractions;
using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.Modules;
using SAE.EdgeRuntime.Modules.Caja.Events;

namespace SAE.EdgeRuntime.Modules.Caja;

public class CajaModule : IEdgeModule
{
    public string Name => "Caja";

    public void Configure(ModuleContext context)
    {
    }

    public void RegisterHandlers(IEventBus bus)
    {
    }

    public bool CanHandle(IEvent evt)
    {
        return evt is CajaSessionOpenedEvent || evt is CajaMovementRegisteredEvent;
    }

    public async Task Handle(IEvent evt)
    {
        if (evt is CajaSessionOpenedEvent opened)
        {
            Console.WriteLine($"[Caja] SESSION OPENED on Terminal {opened.TerminalId} with {opened.InitialAmount:C}");
        }
        else if (evt is CajaMovementRegisteredEvent move)
        {
            Console.WriteLine($"[Caja] MOVEMENT: {move.Type} of {move.Amount:C} ({move.Reason})");
        }
        
        await Task.CompletedTask;
    }
}
