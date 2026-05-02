using Dapper;
using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Modules.Common.Projections;
using SAE.EdgeRuntime.Modules.Caja.Events;

namespace SAE.EdgeRuntime.Modules.Caja.Projections;

public class CajaBalanceProjection : DapperProjectionBase
{
    public CajaBalanceProjection(string connectionString) : base(connectionString) { }

    public override async Task HandleAsync(IEvent @event)
    {
        using var db = CreateConnection();

        if (@event is CajaSessionOpenedEvent opened)
        {
            const string sql = "INSERT INTO read_caja_balances (id, tenant_id, terminal_id, balance) VALUES (@Id, @TenantId, @TerminalId, @Balance) ON CONFLICT (id) DO UPDATE SET balance = EXCLUDED.balance";
            await db.ExecuteAsync(sql, new { Id = opened.AggregateId, TenantId = opened.TenantId, TerminalId = opened.TerminalId, Balance = opened.InitialAmount });
        }
        else if (@event is CajaMovementRegisteredEvent move)
        {
            const string sql = "UPDATE read_caja_balances SET balance = balance + @Amount WHERE id = @Id";
            await db.ExecuteAsync(sql, new { Id = move.AggregateId, Amount = move.Amount });
        }
    }
}
