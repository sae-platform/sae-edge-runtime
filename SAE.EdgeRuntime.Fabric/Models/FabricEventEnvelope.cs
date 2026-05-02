namespace SAE.EdgeRuntime.Fabric.Models;

public sealed class FabricEventEnvelope
{
    public Guid EventId { get; init; }
    public Guid TenantId { get; init; }
    public Guid NodeId { get; init; }

    public string AggregateType { get; init; }
    public Guid AggregateId { get; init; }

    public long Version { get; init; }

    public DateTime OccurredAt { get; init; }

    public string EventType { get; init; }
    public string Payload { get; init; }

    public string Hash { get; init; } // Global idempotency hash

    // OpenTelemetry Distributed Tracing (Observability Layer)
    public string TraceId { get; init; }
    public string SpanId { get; init; }
    public string ParentSpanId { get; init; }
}
