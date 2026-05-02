using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Modules.Catalog.Commands;
using SAE.EdgeRuntime.Modules.Catalog.Events;

namespace SAE.EdgeRuntime.Modules.Catalog.Aggregates;

public sealed class Item : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal TaxRate { get; private set; }
    public bool TrackInventory { get; private set; }

    public IEnumerable<IEvent> Handle(CreateItemCommand cmd)
    {
        if (Version > 0) throw new InvalidOperationException("Item already exists.");
        
        var evt = new ItemCreatedEvent(
            Guid.NewGuid(), cmd.AggregateId, cmd.TenantId, DateTime.UtcNow, 1, 
            cmd.CorrelationId, Guid.NewGuid(),
            cmd.Name, cmd.Sku, cmd.Barcode, cmd.BasePrice, cmd.TaxRate, cmd.TrackInventory
        );
        
        Raise(evt);
        return GetUncommittedEvents();
    }

    public IEnumerable<IEvent> Handle(UpdateItemPriceCommand cmd)
    {
        var evt = new ItemPriceUpdatedEvent(
            Guid.NewGuid(), Id, cmd.TenantId, DateTime.UtcNow, Version + 1,
            cmd.CorrelationId, Guid.NewGuid(), cmd.NewPrice
        );
        
        Raise(evt);
        return GetUncommittedEvents();
    }

    protected override void Apply(IEvent @event)
    {
        if (@event is ItemCreatedEvent created)
        {
            Id = created.AggregateId;
            Name = created.Name;
            Sku = created.Sku;
            Price = created.BasePrice;
            TaxRate = created.TaxRate;
            TrackInventory = created.TrackInventory;
        }
        else if (@event is ItemPriceUpdatedEvent priceUpdated)
        {
            Price = priceUpdated.NewPrice;
        }
    }
}
