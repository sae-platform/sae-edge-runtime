using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SAE.EdgeRuntime.Core.Kernel.Concurrency;

public class TenantThrottler
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _tenantSemaphores = new();
    private readonly int _maxConcurrentEventsPerTenant;

    public TenantThrottler(int maxConcurrentEventsPerTenant = 100)
    {
        _maxConcurrentEventsPerTenant = maxConcurrentEventsPerTenant;
    }

    public async Task WaitAsync(Guid tenantId, CancellationToken ct = default)
    {
        var semaphore = _tenantSemaphores.GetOrAdd(tenantId, _ => new SemaphoreSlim(_maxConcurrentEventsPerTenant, _maxConcurrentEventsPerTenant));
        
        await semaphore.WaitAsync(ct);
    }

    public void Release(Guid tenantId)
    {
        if (_tenantSemaphores.TryGetValue(tenantId, out var semaphore))
        {
            semaphore.Release();
        }
    }
}
