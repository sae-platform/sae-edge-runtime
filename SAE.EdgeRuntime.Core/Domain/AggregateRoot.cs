namespace SAE.EdgeRuntime.Core.Domain;

public abstract class AggregateRoot
{
    private readonly List<IEvent> _changes = new();

    public Guid Id { get; protected set; }
    public int Version { get; protected set; }

    public IEnumerable<IEvent> GetUncommittedEvents() => _changes;

    public void MarkCommitted() => _changes.Clear();

    protected void Raise(IEvent @event)
    {
        Apply(@event);
        _changes.Add(@event);
        Version++;
    }

    protected abstract void Apply(IEvent @event);

    public void ApplyFromHistory(IEvent @event)
    {
        Apply(@event);
        Version++;
    }
}
