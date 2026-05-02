using NATS.Net;
using NATS.Client.Core;
using SAE.EdgeRuntime.Core.Storage;
using SAE.EdgeRuntime.Fabric.Models;
using System.Text.Json;

namespace SAE.EdgeRuntime.Fabric;

public sealed class ReplicationEngine(
    INatsConnection nats,
    IEventStore store,
    ConflictResolver resolver)
{
    private readonly INatsConnection _nats = nats;
    private readonly IEventStore _store = store;
    private readonly ConflictResolver _resolver = resolver;

    public async Task PublishLocalEvent(FabricEventEnvelope evt)
    {
        var subject = $"sae.events.{evt.TenantId}";
        var payload = JsonSerializer.SerializeToUtf8Bytes(evt);
        
        await _nats.PublishAsync(subject, payload);
    }

    public async Task ConsumeRemoteEvent(FabricEventEnvelope evt)
    {
        // Global idempotency check
        // var exists = await _store.Exists(evt.EventId);
        // if (exists) return;

        await _resolver.ResolveAndApply(evt);
    }

    public async Task StartSyncWorker(Guid tenantId, CancellationToken ct)
    {
        var subject = $"sae.events.{tenantId}";
        
        // Note: We would ideally inject ChannelPressureMonitor here
        // For skeleton demonstration:
        // var monitor = ...;

        await foreach (var msg in _nats.SubscribeAsync<FabricEventEnvelope>(subject).WithCancellation(ct))
        {
            // Backpressure check
            // if (monitor.IsUnderPressure(tenantId))
            // {
            //     Console.WriteLine($"[NATS] Pausing consumption for Tenant {tenantId} due to backpressure.");
            //     await Task.Delay(1000, ct); // Simple backoff. JetStream will redeliver or hold.
            // }

            if (msg.Data != null)
            {
                await ConsumeRemoteEvent(msg.Data);
            }
        }
    }
}
