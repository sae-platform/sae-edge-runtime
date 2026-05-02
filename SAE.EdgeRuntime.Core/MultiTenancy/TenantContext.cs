namespace SAE.EdgeRuntime.Core.MultiTenancy;

public sealed class TenantContext
{
    public Guid TenantId { get; }

    public TenantContext(Guid tenantId)
    {
        TenantId = tenantId;
    }
}
