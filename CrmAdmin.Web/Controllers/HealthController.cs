using Microsoft.AspNetCore.Mvc;

namespace CrmAdmin.Web.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new {
            ok = true,
            timeUtc = DateTime.UtcNow,
            env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
    }
}
