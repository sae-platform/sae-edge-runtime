using Microsoft.Extensions.Logging;
using SAE.EdgeRuntime.Core.Kernel;
using SAE.EdgeRuntime.Core.MultiTenancy;

namespace SAE.EdgeRuntime.Modules.Fiscal;

public sealed class FiscalEngine(
    IEnumerable<IFiscalAdapter> adapters,
    TerminalIdentityManager terminalManager,
    ILogger<FiscalEngine> logger)
{
    private readonly IEnumerable<IFiscalAdapter> _adapters = adapters;
    private readonly TerminalIdentityManager _terminalManager = terminalManager;
    private readonly ILogger<FiscalEngine> _logger = logger;

    public async Task<FiscalDocumentResponse> ProcessSaleAsync(TenantContext tenant, decimal totalAmount, string payload, string countryCode)
    {
        var adapter = _adapters.FirstOrDefault(a => a.CountryCode == countryCode);
        
        if (adapter == null)
        {
            _logger.LogError("No fiscal adapter found for country code {CountryCode}", countryCode);
            return new FiscalDocumentResponse(false, null, "Adapter not found");
        }

        var terminalId = _terminalManager.GetTerminalId();
        
        // Note: In a real system, the sequence number should be retrieved from a reliable persistent sequence generator.
        long sequenceNumber = DateTime.UtcNow.Ticks % 1000000; 

        var request = new FiscalDocumentRequest(terminalId, sequenceNumber, totalAmount, payload);
        
        return await adapter.SignAndSubmitAsync(request);
    }
}
