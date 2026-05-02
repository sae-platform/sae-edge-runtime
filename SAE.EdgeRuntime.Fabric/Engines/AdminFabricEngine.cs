using NATS.Net;
using NATS.Client.Core;
using SAE.EdgeRuntime.Fabric.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SAE.EdgeRuntime.Fabric.Engines;

public sealed class AdminFabricEngine(
    INatsConnection nats,
    ILogger<AdminFabricEngine> logger)
{
    private readonly INatsConnection _nats = nats;
    private readonly ILogger<AdminFabricEngine> _logger = logger;

    // Pushes operational business events (sales, new clients, audit logs) upstream for global analytics
    public async Task PushAnalyticsEventAsync(FabricEventEnvelope evt)
    {
        // Route all transactional events to the global events stream
        var subject = $"sae.events.{evt.TenantId}.{evt.AggregateType.ToLowerInvariant()}";
        var payload = JsonSerializer.SerializeToUtf8Bytes(evt);
        
        await _nats.PublishAsync(subject, payload);
        _logger.LogDebug("Pushed analytics event {EventId} to Admin Fabric on subject {Subject}", evt.EventId, subject);
    }
}
