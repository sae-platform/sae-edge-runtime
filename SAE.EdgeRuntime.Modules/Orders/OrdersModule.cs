using System;
using System.Threading.Tasks;
using SAE.EdgeRuntime.Core.Abstractions;
using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.Modules;

using SAE.EdgeRuntime.Modules.Orders.Events;

namespace SAE.EdgeRuntime.Modules.Orders;

public class OrdersModule : IEdgeModule
{
    public string Name => "Orders";

    public void Configure(ModuleContext context)
    {
        // Initialize state, dependencies
    }

    public void RegisterHandlers(IEventBus bus)
    {
        // Example registration. In a real system, the EventBus might not be used this way 
        // if ModuleEventRouter handles it, but this aligns with the blueprint.
        // bus.Subscribe<OrderCreatedEvent>(Handle);
    }

    public bool CanHandle(IEvent evt)
    {
        return evt is OrderStartedEvent || evt is OrderClosedEvent;
    }

    public async Task Handle(IEvent evt)
    {
        if (evt is OrderStartedEvent started)
        {
            Console.WriteLine($"[Orders] Order Started: {started.AggregateId} for Customer {started.CustomerId}");
        }
        else if (evt is OrderClosedEvent closed)
        {
            Console.WriteLine($"[Orders] Order Closed: {closed.AggregateId}");
        }
        
        await Task.CompletedTask;
    }
}
