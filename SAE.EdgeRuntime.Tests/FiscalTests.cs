using Moq;
using Microsoft.Extensions.Logging;
using SAE.EdgeRuntime.Modules.Fiscal;
using SAE.EdgeRuntime.Core.Domain;
using SAE.EdgeRuntime.Core.MultiTenancy;
using SAE.EdgeRuntime.Core.Kernel;
using Xunit;

namespace SAE.EdgeRuntime.Tests;

public class FiscalTests
{
    [Fact]
    public async Task FiscalEngine_ShouldProcessSale_UsingCostaRicaAdapter()
    {
        // Arrange
        var adapterLoggerMock = new Mock<ILogger<CostaRicaFiscalAdapter>>();
        var adapter = new CostaRicaFiscalAdapter(adapterLoggerMock.Object);
        
        var terminalMock = new Mock<TerminalIdentityManager>();
        terminalMock.Setup(t => t.GetTerminalId()).Returns("TERM-001");
        
        var loggerMock = new Mock<ILogger<FiscalEngine>>();
        
        var engine = new FiscalEngine(new List<IFiscalAdapter> { adapter }, terminalMock.Object, loggerMock.Object);
        var tenant = new TenantContext(Guid.NewGuid());
        
        // Act
        var result = await engine.ProcessSaleAsync(tenant, 100m, "{}", "CR");

        // Assert
        Assert.True(result.Success);
        Assert.StartsWith("506", result.FiscalReceiptNumber ?? "");
    }
}
