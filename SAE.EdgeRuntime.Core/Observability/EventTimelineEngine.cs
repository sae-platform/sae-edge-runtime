using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;
using SAE.EdgeRuntime.Core.Storage;

namespace SAE.EdgeRuntime.Core.Observability;

public class EventTimelineEngine
{
    private readonly IEventStore _store;

    public EventTimelineEngine(IEventStore store)
    {
        _store = store;
    }

    public async Task<IEnumerable<TimelineEntry>> GetTimelineAsync(TenantContext tenant, Guid aggregateId)
    {
        var events = await _store.LoadEvents(tenant, aggregateId);
        
        return events.Select(e => new TimelineEntry
        {
            EventId = e.EventId,
            Timestamp = e.OccurredAt,
            EventType = e.GetType().Name,
            Summary = GetSummary(e)
        }).OrderBy(t => t.Timestamp);
    }

    private string GetSummary(IEvent e)
    {
        // Logic to extract a human-readable summary from the event
        return $"Event {e.GetType().Name} occurred at {e.OccurredAt}";
    }
}

public class TimelineEntry
{
    public Guid EventId { get; set; }
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}
