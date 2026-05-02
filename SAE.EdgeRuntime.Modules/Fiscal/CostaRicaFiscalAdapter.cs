using Microsoft.Extensions.Logging;

namespace SAE.EdgeRuntime.Modules.Fiscal;

public sealed class CostaRicaFiscalAdapter(ILogger<CostaRicaFiscalAdapter> logger) : IFiscalAdapter
{
    private readonly ILogger<CostaRicaFiscalAdapter> _logger = logger;
    public string CountryCode => "CR";

    public async Task<FiscalDocumentResponse> SignAndSubmitAsync(FiscalDocumentRequest request)
    {
        _logger.LogInformation("Submitting document to Hacienda CR for Terminal {TerminalId}", request.TerminalId);
        
        // In a real scenario, this would format the XML, sign it, and call the Ministerio de Hacienda API.
        await Task.Delay(100); // Simulate network call
        
        // Mock success
        var fakeClave = $"50601012600{request.TerminalId}{request.SequenceNumber:D10}1";
        
        return new FiscalDocumentResponse(true, fakeClave, null);
    }
}
