using Dapper;
using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Modules.Common.Projections;
using SAE.EdgeRuntime.Modules.Catalog.Events;

namespace SAE.EdgeRuntime.Modules.Catalog.Projections;

public class ProductCatalogProjection : DapperProjectionBase
{
    public ProductCatalogProjection(string connectionString) : base(connectionString) { }

    public override async Task HandleAsync(IEvent @event)
    {
        using var db = CreateConnection();

        if (@event is ItemCreatedEvent created)
        {
            const string sql = @"
                INSERT INTO read_catalog_items (id, tenant_id, name, sku, price, tax_rate, track_inventory, last_updated_at)
                VALUES (@Id, @TenantId, @Name, @Sku, @Price, @TaxRate, @TrackInventory, @UpdatedAt)
                ON CONFLICT (id) DO UPDATE SET 
                    name = EXCLUDED.name, 
                    price = EXCLUDED.price, 
                    last_updated_at = EXCLUDED.last_updated_at;";

            await db.ExecuteAsync(sql, new
            {
                Id = created.AggregateId,
                TenantId = created.TenantId,
                Name = created.Name,
                Sku = created.Sku,
                Price = created.BasePrice,
                TaxRate = created.TaxRate,
                TrackInventory = created.TrackInventory,
                UpdatedAt = created.OccurredAt
            });
        }
        else if (@event is ItemPriceUpdatedEvent priceUpdated)
        {
            const string sql = "UPDATE read_catalog_items SET price = @Price, last_updated_at = @UpdatedAt WHERE id = @Id";
            await db.ExecuteAsync(sql, new { Id = priceUpdated.AggregateId, Price = priceUpdated.NewPrice, UpdatedAt = priceUpdated.OccurredAt });
        }
    }
}
