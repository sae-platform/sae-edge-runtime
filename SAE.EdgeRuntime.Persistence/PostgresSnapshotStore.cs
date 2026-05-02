using Dapper;
using Npgsql;
using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;
using SAE.EdgeRuntime.Core.Storage;
using System.Text.Json;

namespace SAE.EdgeRuntime.Persistence;

public sealed class PostgresSnapshotStore(string connectionString) : ISnapshotStore
{
    private readonly string _connectionString = connectionString;

    public async Task SaveAsync(TenantContext tenant, Guid aggregateId, AggregateRoot aggregate)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        var json = JsonSerializer.Serialize((object)aggregate);
        
        const string sql = @"
            INSERT INTO edge_snapshots (tenant_id, aggregate_id, version, data, updated_at)
            VALUES (@TenantId, @AggregateId, @Version, @Data::jsonb, @UpdatedAt)
            ON CONFLICT (tenant_id, aggregate_id) 
            DO UPDATE SET version = EXCLUDED.version, data = EXCLUDED.data, updated_at = EXCLUDED.updated_at;";

        await conn.ExecuteAsync(sql, new 
        { 
            TenantId = tenant.TenantId,
            AggregateId = aggregateId,
            Version = aggregate.Version,
            Data = json,
            UpdatedAt = DateTime.UtcNow
        });
    }

    public async Task<(int Version, T Aggregate)?> LoadAsync<T>(TenantContext tenant, Guid aggregateId) where T : AggregateRoot, new()
    {
        using var conn = new NpgsqlConnection(_connectionString);
        
        const string sql = @"
            SELECT version, data 
            FROM edge_snapshots 
            WHERE tenant_id = @TenantId AND aggregate_id = @AggregateId";

        var row = await conn.QueryFirstOrDefaultAsync(sql, new 
        { 
            TenantId = tenant.TenantId, 
            AggregateId = aggregateId 
        });

        if (row == null) return null;

        var aggregate = JsonSerializer.Deserialize<T>((string)row.data);
        return (row.version, aggregate!);
    }
}
