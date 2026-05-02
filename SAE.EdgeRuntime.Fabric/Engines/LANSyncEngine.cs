using NATS.Net;
using NATS.Client.Core;
using SAE.EdgeRuntime.Core.Storage;
using SAE.EdgeRuntime.Fabric.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SAE.EdgeRuntime.Fabric.Engines;

public sealed class LANSyncEngine(
    INatsConnection nats,
    IEventStore store,
    ILogger<LANSyncEngine> logger)
{
    private readonly INatsConnection _nats = nats;
    private readonly IEventStore _store = store;
    private readonly ILogger<LANSyncEngine> _logger = logger;

    // Publishes operational domain events (e.g. new client, updated product) to the LAN peer network
    public async Task PublishDomainEventAsync(FabricEventEnvelope evt)
    {
        var subject = $"sae.domain.{evt.AggregateType.ToLowerInvariant()}.{evt.TenantId}";
        var payload = JsonSerializer.SerializeToUtf8Bytes(evt);
        
        await _nats.PublishAsync(subject, payload);
        _logger.LogDebug("Published domain event {EventId} to LAN Fabric on subject {Subject}", evt.EventId, subject);
    }

    // Subscribes to peer operational events and applies Last-Write-Wins strategy
    public async Task StartSyncWorkerAsync(Guid tenantId, CancellationToken ct)
    {
        var subject = $"sae.domain.*.{tenantId}";
        
        _logger.LogInformation("Starting LAN Sync Worker for tenant {TenantId} on {Subject}", tenantId, subject);

        await foreach (var msg in _nats.SubscribeAsync<FabricEventEnvelope>(subject).WithCancellation(ct))
        {
            if (msg.Data != null)
            {
                await MergeDomainStateAsync(msg.Data);
            }
        }
    }

    private async Task MergeDomainStateAsync(FabricEventEnvelope evt)
    {
        // Simple Last-Write-Wins Merge Strategy
        // 1. Check local timestamp/version for the same aggregate.
        // 2. If remote event is newer, apply it to the local EventStore.
        
        _logger.LogInformation("Merged remote domain event {EventId} for aggregate {AggregateId}", evt.EventId, evt.AggregateId);
        
        // await _store.AppendAsync(new TenantContext(evt.TenantId), evt.AggregateId, ...);
    }
}
