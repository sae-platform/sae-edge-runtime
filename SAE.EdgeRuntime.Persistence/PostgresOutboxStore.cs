using Dapper;
using Npgsql;
using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;
using SAE.EdgeRuntime.Core.Storage;
using System.Text.Json;

namespace SAE.EdgeRuntime.Persistence;

public class PostgresOutboxStore : IOutboxStore
{
    private readonly string _connectionString;

    public PostgresOutboxStore(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task StoreAsync(TenantContext tenant, IEvent @event)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        const string sql = @"
            INSERT INTO edge_outbox (id, event_id, event_type, payload, tenant_id, created_at, is_published)
            VALUES (@Id, @EventId, @EventType, @Payload, @TenantId, @CreatedAt, false)";

        var message = new
        {
            Id = Guid.NewGuid(),
            EventId = @event.EventId,
            EventType = @event.GetType().Name,
            Payload = JsonSerializer.Serialize(@event),
            TenantId = tenant.TenantId,
            CreatedAt = DateTime.UtcNow
        };

        await conn.ExecuteAsync(sql, message);
    }

    public async Task<IEnumerable<OutboxMessage>> GetPendingAsync(TenantContext tenant, int batchSize = 100)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        const string sql = @"
            SELECT id, event_id, event_type, payload, created_at, is_published
            FROM edge_outbox
            WHERE tenant_id = @TenantId AND is_published = false
            ORDER BY created_at ASC
            LIMIT @BatchSize";

        return await conn.QueryAsync<OutboxMessage>(sql, new { TenantId = tenant.TenantId, BatchSize = batchSize });
    }

    public async Task MarkAsPublishedAsync(Guid messageId)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        const string sql = "UPDATE edge_outbox SET is_published = true WHERE id = @Id";
        await conn.ExecuteAsync(sql, new { Id = messageId });
    }
}
