using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SAE.EdgeRuntime.Core.Storage;
using SAE.EdgeRuntime.Core.MultiTenancy;

namespace SAE.EdgeRuntime.Sync;

public sealed class OutboxProcessor(
    IOutboxStore outboxStore,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private readonly IOutboxStore _outboxStore = outboxStore;
    private readonly ILogger<OutboxProcessor> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor started.");

        // For the skeleton, we simulate a global tenant context or loop through tenants
        var tenantContext = new TenantContext(Guid.Empty);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var messages = await _outboxStore.GetPendingAsync(tenantContext);

                if (messages.Any())
                {
                    _logger.LogInformation("Processing {Count} outbox messages.", messages.Count());

                    // TODO: Send to Cloud Sync Client
                    // await _syncClient.SendAsync(messages);

                    foreach (var msg in messages)
                    {
                        await _outboxStore.MarkAsPublishedAsync(msg.Id);
                    }
                    
                    _logger.LogInformation("Marked messages as processed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages.");
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}
