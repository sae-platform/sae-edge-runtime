using SAE.EdgeRuntime.Core.MultiTenancy;

namespace SAE.EdgeRuntime.Core.Kernel;

public class TerminalIdentityManager
{
    private string _terminalId = "00000";

    public void Initialize(string terminalId)
    {
        if (terminalId.Length != 5 || !terminalId.All(char.IsDigit))
            throw new ArgumentException("Terminal ID must be exactly 5 digits.");
        
        _terminalId = terminalId;
    }

    public virtual string GetTerminalId() => _terminalId;

    public string GenerateFiscalNumber(long sequenceNumber)
    {
        // Example: 001-00001-0000001234
        // 001 (Branch/System) - 00001 (Terminal) - 0000001234 (Sequence)
        return $"001-{_terminalId}-{sequenceNumber:D10}";
    }
}
