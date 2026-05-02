using System;
using SAE.EdgeRuntime.Core.MultiTenancy;

namespace SAE.EdgeRuntime.Core.Kernel.Concurrency;

public class ChannelPressureMonitor
{
    private readonly TenantChannelRegistry _registry;
    private const int HighWaterMark = 5000;
    private const int LowWaterMark = 1000;

    public ChannelPressureMonitor(TenantChannelRegistry registry)
    {
        _registry = registry;
    }

    public bool IsUnderPressure(Guid tenantId)
    {
        var channel = _registry.GetChannel(tenantId);
        
        // Check if the number of queued items exceeds the high water mark
        // This is a naive implementation; System.Threading.Channels doesn't expose Count directly 
        // without reflection or wrapper, so in a real app we'd wrap the Channel to track Count.
        // Assuming a wrapper exists that tracks depth:
        
        // int currentDepth = channel.Count; 
        int currentDepth = 0; // Placeholder
        
        if (currentDepth >= HighWaterMark)
        {
            Console.WriteLine($"[Backpressure] Tenant {tenantId} is under high pressure (Depth: {currentDepth}). Signaling throttle.");
            return true;
        }

        return false;
    }

    public bool IsPressureRelieved(Guid tenantId)
    {
        // Conceptual: int currentDepth = channel.Count;
        int currentDepth = 0; // Placeholder

        if (currentDepth <= LowWaterMark)
        {
            return true;
        }

        return false;
    }
}
