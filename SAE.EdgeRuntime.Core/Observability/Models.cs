using System;
using System.Collections.Generic;

namespace SAE.EdgeRuntime.Core.Observability;

public class TimedEvent
{
    public Guid EventId { get; set; }
    public Guid TenantId { get; set; }
    public string Module { get; set; }
    public DateTime OccurredAt { get; set; }
    public long Sequence { get; set; }
    public string EventType { get; set; }
    public object Payload { get; set; }
    public string CorrelationId { get; set; }
}

public class EventNode
{
    public string EventType { get; set; }
    public Guid EventId { get; set; }
    public List<Guid> Next { get; set; } = new();
}

public class TraceGraph
{
    public string CorrelationId { get; set; }
    public List<EventNode> Nodes { get; set; } = new();
}
