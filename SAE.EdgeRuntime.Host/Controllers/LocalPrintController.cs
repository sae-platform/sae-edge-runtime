using Microsoft.AspNetCore.Mvc;
using SAE.EdgeRuntime.Modules.Hardware;
using SAE.Contracts.Hardware;

namespace SAE.EdgeRuntime.Host.Controllers;

[ApiController]
[Route("api/hardware/print")]
public class LocalPrintController(HardwareModule hardware) : ControllerBase
{
    private readonly HardwareModule _hardware = hardware;

    [HttpPost]
    public async Task<IActionResult> Print([FromBody] PrintApiRequest request)
    {
        // Enqueue to hardware module
        await _hardware.PrintAsync(
            Guid.NewGuid().ToString(), 
            request.Target, 
            request.TemplateId, 
            request.Data);

        return Ok(new { status = "Enqueued" });
    }
}
