using Microsoft.Extensions.DependencyInjection;
using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;

namespace SAE.EdgeRuntime.Core.Kernel;

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task Handle(TCommand command, TenantContext tenant);
}

public class CommandDispatcher(IServiceProvider sp)
{
    private readonly IServiceProvider _sp = sp;

    public async Task Dispatch<TCommand>(TCommand command, TenantContext tenant)
        where TCommand : ICommand
    {
        var handler = _sp.GetRequiredService<ICommandHandler<TCommand>>();
        await handler.Handle(command, tenant);
    }
}
