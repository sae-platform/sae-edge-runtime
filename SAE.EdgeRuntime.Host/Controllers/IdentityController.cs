using Microsoft.AspNetCore.Mvc;
using SAE.EdgeRuntime.Modules.Identity.Persistence;
using SAE.EdgeRuntime.Modules.Identity.Domain;

namespace SAE.EdgeRuntime.Host.Controllers;

[ApiController]
[Route("api/identity")]
public class IdentityController(StaffRepository repository) : ControllerBase
{
    private readonly StaffRepository _repository = repository;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var staff = await _repository.GetByPinAsync(request.Pin);
        
        if (staff == null)
            return Unauthorized(new LoginResponse(false, null, null, null));

        var permissions = staff.Roles.SelectMany(r => r.Permissions).Distinct().ToList();
        
        return Ok(new LoginResponse(
            true, 
            staff.FullName, 
            permissions, 
            "local-edge-session-token" // In a real scenario, generate a JWT
        ));
    }
}
