using Microsoft.Extensions.DependencyInjection;
using SAE.EdgeRuntime.Core.Events;
using SAE.EdgeRuntime.Core.Kernel;
using SAE.EdgeRuntime.Core.MultiTenancy;
using SAE.EdgeRuntime.Core.Recovery;
using SAE.EdgeRuntime.Core.Storage;
using SAE.EdgeRuntime.Core.Projections;

namespace SAE.EdgeRuntime.Core;

public static class KernelBootstrap
{
    public static IServiceCollection AddEdgeKernel(this IServiceCollection services)
    {
        services.AddSingleton<TenantChannelRegistry>();
        services.AddSingleton<IDomainEventPublisher, DomainEventPublisher>();
        services.AddSingleton<CommandDispatcher>();
        services.AddSingleton<ExecutionPipeline>();
        services.AddSingleton<RecoveryEngine>();
        services.AddSingleton<SnapshotEngine>();
        services.AddSingleton<ProjectionEngine>();
        
        // Concurrency components
        services.AddSingleton<TenantRuntimeManager>();
        // services.AddSingleton<TenantDispatcherLoopFactory>(); // Missing factory

        return services;
    }
}
