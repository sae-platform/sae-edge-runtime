using System.Data;
using Npgsql;
using SAE.EdgeRuntime.Core.Projections;
using SAE.EdgeRuntime.Core.Domain;

namespace SAE.EdgeRuntime.Modules.Common.Projections;

public abstract class DapperProjectionBase : IProjection
{
    protected readonly string ConnectionString;

    protected DapperProjectionBase(string connectionString)
    {
        ConnectionString = connectionString;
    }

    protected IDbConnection CreateConnection() => new NpgsqlConnection(ConnectionString);

    public abstract Task HandleAsync(IEvent @event);
}
