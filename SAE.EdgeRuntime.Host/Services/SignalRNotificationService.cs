using Microsoft.AspNetCore.SignalR;
using SAE.EdgeRuntime.Host.Hubs;
using SAE.EdgeRuntime.Modules.Hardware.Abstractions;

namespace SAE.EdgeRuntime.Host.Services;

public class SignalRNotificationService(IHubContext<PrintHub> hubContext) : IPrintNotificationService
{
    private readonly IHubContext<PrintHub> _hubContext = hubContext;

    public async Task NotifyStatusAsync(string jobId, string status, int attempts, string? error = null)
    {
        await _hubContext.Clients.All.SendAsync("print-update", new {
            jobId,
            status,
            attempts,
            error,
            timestamp = DateTime.UtcNow
        });
    }
}
