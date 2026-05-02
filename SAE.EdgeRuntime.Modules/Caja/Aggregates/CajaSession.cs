using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Modules.Caja.Commands;
using SAE.EdgeRuntime.Modules.Caja.Events;

namespace SAE.EdgeRuntime.Modules.Caja.Aggregates;

public sealed class CajaSession : AggregateRoot
{
    public string TerminalId { get; private set; } = string.Empty;
    public decimal CurrentBalance { get; private set; }
    public bool IsOpen { get; private set; }

    public IEnumerable<IEvent> Handle(OpenCajaSessionCommand cmd)
    {
        if (Version > 0) throw new InvalidOperationException("Session already exists.");
        
        Raise(new CajaSessionOpenedEvent(
            Guid.NewGuid(), cmd.AggregateId, cmd.TenantId, DateTime.UtcNow, 1,
            cmd.CorrelationId, Guid.NewGuid(), cmd.TerminalId, cmd.InitialAmount
        ));
        
        return GetUncommittedEvents();
    }

    public IEnumerable<IEvent> Handle(RegisterCajaMovementCommand cmd)
    {
        if (!IsOpen) throw new InvalidOperationException("Caja session is not open.");
        
        Raise(new CajaMovementRegisteredEvent(
            Guid.NewGuid(), Id, cmd.TenantId, DateTime.UtcNow, Version + 1,
            cmd.CorrelationId, Guid.NewGuid(), cmd.Amount, cmd.Type, cmd.Reason
        ));
        
        return GetUncommittedEvents();
    }

    protected override void Apply(IEvent @event)
    {
        if (@event is CajaSessionOpenedEvent opened)
        {
            Id = opened.AggregateId;
            TerminalId = opened.TerminalId;
            CurrentBalance = opened.InitialAmount;
            IsOpen = true;
        }
        else if (@event is CajaMovementRegisteredEvent move)
        {
            CurrentBalance += move.Amount;
        }
    }
}
