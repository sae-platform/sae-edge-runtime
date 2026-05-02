using System;
using SAE.EdgeRuntime.Core.Abstractions;
using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Core.Modules;

public interface IEdgeModule
{
    string Name { get; }

    void Configure(ModuleContext context);

    void RegisterHandlers(IEventBus bus);

    bool CanHandle(IEvent evt);

    Task Handle(IEvent evt);
}

public class ModuleContext
{
    public Guid TenantId { get; }
    public IServiceProvider Services { get; }
    public IEventBus EventBus { get; }
    public ModuleStateStore StateStore { get; }

    public ModuleContext(Guid tenantId, IServiceProvider services, IEventBus eventBus, ModuleStateStore stateStore)
    {
        TenantId = tenantId;
        Services = services;
        EventBus = eventBus;
        StateStore = stateStore;
    }
}

public class ModuleStateStore
{
    private readonly Dictionary<string, object> _state = new();

    public T Get<T>(string key)
    {
        return (T)_state[key];
    }

    public void Set(string key, object value)
    {
        _state[key] = value;
    }
}
