using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;

namespace SAE.EdgeRuntime.Core.Storage;

public interface IOutboxStore
{
    Task StoreAsync(TenantContext tenant, IEvent @event);
    Task<IEnumerable<OutboxMessage>> GetPendingAsync(TenantContext tenant, int batchSize = 100);
    Task MarkAsPublishedAsync(Guid messageId);
}

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsPublished { get; set; }
}
