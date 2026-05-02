using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Modules.Billing.Commands;
using SAE.EdgeRuntime.Modules.Billing.Events;

namespace SAE.EdgeRuntime.Modules.Billing.Aggregates;

public sealed class InvoiceAggregate : AggregateRoot
{
    public string FiscalNumber { get; private set; } = string.Empty;
    public decimal Total { get; private set; }

    public IEnumerable<IEvent> Handle(CreateInvoiceCommand cmd)
    {
        if (Version > 0) throw new InvalidOperationException("Invoice already exists.");

        var subtotal = cmd.Lines.Sum(l => l.UnitPrice * l.Quantity);
        var totalTax = cmd.Lines.Sum(l => (l.UnitPrice * l.Quantity) * l.TaxRate);
        var total = subtotal + totalTax;

        // Simplified correlative for now
        var fiscalNumber = $"FAC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

        var evt = new InvoiceGeneratedEvent(
            Guid.NewGuid(), cmd.AggregateId, cmd.TenantId, DateTime.UtcNow, 1,
            cmd.CorrelationId, Guid.NewGuid(), cmd.OrderId, fiscalNumber,
            subtotal, totalTax, total, cmd.PaymentMethod
        );

        Raise(evt);
        return GetUncommittedEvents();
    }

    protected override void Apply(IEvent @event)
    {
        if (@event is InvoiceGeneratedEvent generated)
        {
            Id = generated.AggregateId;
            FiscalNumber = generated.FiscalNumber;
            Total = generated.TotalWithTax;
        }
    }
}
