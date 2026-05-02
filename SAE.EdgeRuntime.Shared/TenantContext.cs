namespace SAE.EdgeRuntime.Shared.Context;

public static class TenantContext
{
    private static readonly AsyncLocal<Guid> _currentTenantId = new();

    public static Guid TenantId
    {
        get => _currentTenantId.Value;
        set => _currentTenantId.Value = value;
    }
}
