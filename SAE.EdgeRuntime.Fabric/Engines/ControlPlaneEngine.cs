using NATS.Net;
using NATS.Client.Core;
using SAE.EdgeRuntime.Fabric.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SAE.EdgeRuntime.Fabric.Engines;

public sealed class ControlPlaneEngine(
    INatsConnection nats,
    ILogger<ControlPlaneEngine> logger)
{
    private readonly INatsConnection _nats = nats;
    private readonly ILogger<ControlPlaneEngine> _logger = logger;

    // Subscribes to global configuration and governance events from the Control Plane
    public async Task StartControlWorkerAsync(CancellationToken ct)
    {
        // Listen to all control plane messages. Note: We might need to filter by node/tenant in a real scenario.
        var subject = "sae.control.*";
        
        _logger.LogInformation("Starting Control Plane Worker on {Subject}", subject);

        await foreach (var msg in _nats.SubscribeAsync<FabricEventEnvelope>(subject).WithCancellation(ct))
        {
            if (msg.Data != null)
            {
                await ApplyControlCommandAsync(msg.Data);
            }
        }
    }

    private Task ApplyControlCommandAsync(FabricEventEnvelope evt)
    {
        _logger.LogInformation("Received Control Plane Command: {EventType} for Node {NodeId}", evt.EventType, evt.NodeId);
        
        // Handle different types of control events:
        // - User/Role updates
        // - Feature Flag toggles
        // - Global configuration changes (Tax, Currency)
        // - Terminal ID assignment
        
        return Task.CompletedTask;
    }
}
