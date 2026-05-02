namespace SAE.EdgeRuntime.Modules.Fiscal;

public interface IFiscalAdapter
{
    string CountryCode { get; }
    Task<FiscalDocumentResponse> SignAndSubmitAsync(FiscalDocumentRequest request);
}

public record FiscalDocumentRequest(
    string TerminalId,
    long SequenceNumber,
    decimal TotalAmount,
    string Payload
);

public record FiscalDocumentResponse(
    bool Success,
    string FiscalReceiptNumber, // e.g., Clave Hacienda
    string ErrorMessage
);
