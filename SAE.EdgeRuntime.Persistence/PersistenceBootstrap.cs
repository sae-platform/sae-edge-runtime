using Microsoft.Extensions.DependencyInjection;
using SAE.EdgeRuntime.Core.Storage;

namespace SAE.EdgeRuntime.Persistence;

public static class PersistenceBootstrap
{
    public static IServiceCollection AddEdgePersistence(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IEventStore>(sp => new PostgresEventStore(connectionString));
        services.AddSingleton<IOutboxStore>(sp => new PostgresOutboxStore(connectionString));
        services.AddSingleton<ISnapshotStore>(sp => new PostgresSnapshotStore(connectionString));
        
        return services;
    }
}
