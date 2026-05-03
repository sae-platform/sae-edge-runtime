using Microsoft.AspNetCore.SignalR;

namespace SAE.EdgeRuntime.Host.Hubs;

public class PrintHub : Hub
{
    // Clients can join groups based on TenantId if needed
    public async Task SubscribeToJob(string jobId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, jobId);
    }
}
