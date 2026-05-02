using Dapper;
using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Modules.Common.Projections;
using SAE.EdgeRuntime.Modules.Orders.Events;

namespace SAE.EdgeRuntime.Modules.Orders.Projections;

public class OrderSummaryProjection : DapperProjectionBase
{
    public OrderSummaryProjection(string connectionString) : base(connectionString) { }

    public override async Task HandleAsync(IEvent @event)
    {
        using var db = CreateConnection();

        if (@event is OrderStartedEvent started)
        {
            const string sql = "INSERT INTO read_order_summaries (id, tenant_id, total_amount, total_tax, is_closed) VALUES (@Id, @TenantId, 0, 0, false) ON CONFLICT (id) DO NOTHING";
            await db.ExecuteAsync(sql, new { Id = started.AggregateId, TenantId = started.TenantId });
        }
        else if (@event is OrderClosedEvent closed)
        {
            const string sql = "UPDATE read_order_summaries SET is_closed = true WHERE id = @Id";
            await db.ExecuteAsync(sql, new { Id = closed.AggregateId });
        }
    }
}
