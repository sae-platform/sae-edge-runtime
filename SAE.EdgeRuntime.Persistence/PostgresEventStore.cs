using System.Text.Json;
using Dapper;
using Npgsql;
using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;
using SAE.EdgeRuntime.Core.Storage;

namespace SAE.EdgeRuntime.Persistence;

public sealed class PostgresEventStore(string connectionString) : IEventStore
{
    private readonly string _connectionString = connectionString;

    public async Task AppendAsync(TenantContext tenant, Guid aggregateId, IEvent @event)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        const string sql = @"
            INSERT INTO edge_events (id, aggregate_id, tenant_id, data, type, version, occurred_at)
            VALUES (@Id, @AggregateId, @TenantId, @Data::jsonb, @Type, @Version, @OccurredAt)";

        var data = JsonSerializer.Serialize(@event, @event.GetType());
        await connection.ExecuteAsync(sql, new
        {
            Id = @event.EventId,
            AggregateId = aggregateId,
            TenantId = tenant.TenantId,
            Data = data,
            Type = @event.GetType().AssemblyQualifiedName,
            Version = @event.Version,
            OccurredAt = @event.OccurredAt
        });
    }

    public async Task<IEnumerable<IEvent>> LoadEvents(TenantContext tenant, Guid aggregateId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        const string sql = "SELECT data, type FROM edge_events WHERE aggregate_id = @AggregateId AND tenant_id = @TenantId ORDER BY version ASC";
        
        var rows = await connection.QueryAsync<(string Data, string Type)>(sql, new { AggregateId = aggregateId, TenantId = tenant.TenantId });
        
        var events = new List<IEvent>();
        foreach (var row in rows)
        {
            var type = Type.GetType(row.Type);
            if (type != null)
            {
                var @event = JsonSerializer.Deserialize(row.Data, type) as IEvent;
                if (@event != null) events.Add(@event);
            }
        }
        
        return events;
    }
}
