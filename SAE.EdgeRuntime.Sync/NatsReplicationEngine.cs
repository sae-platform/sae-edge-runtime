using Microsoft.Extensions.Logging;
using NATS.Net;
using NATS.Client.Core;
using SAE.EdgeRuntime.Core.MultiTenancy;
using SAE.EdgeRuntime.Core.Storage;

namespace SAE.EdgeRuntime.Sync;

public class NatsReplicationEngine
{
    private readonly IOutboxStore _outbox;
    private readonly INatsConnection _nats;
    private readonly ILogger<NatsReplicationEngine> _logger;

    public NatsReplicationEngine(IOutboxStore outbox, INatsConnection nats, ILogger<NatsReplicationEngine> logger)
    {
        _outbox = outbox;
        _nats = nats;
        _logger = logger;
    }

    public async Task StartReplicationAsync(TenantContext tenant, CancellationToken ct)
    {
        _logger.LogInformation("Starting replication for tenant {TenantId}...", tenant.TenantId);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var messages = await _outbox.GetPendingAsync(tenant);
                foreach (var msg in messages)
                {
                    // Publish to NATS JetStream
                    // Subject pattern: sae.edge.{tenant}.{module}.{eventType}
                    var subject = $"sae.edge.{tenant.TenantId}.{msg.EventType}";
                    await _nats.PublishAsync(subject, msg.Payload, cancellationToken: ct);

                    // Mark as published in local DB
                    await _outbox.MarkAsPublishedAsync(msg.Id);
                    
                    _logger.LogDebug("Replicated event {EventId} to NATS", msg.EventId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in replication loop for tenant {TenantId}", tenant.TenantId);
            }

            await Task.Delay(1000, ct); // Polling interval
        }
    }
}
