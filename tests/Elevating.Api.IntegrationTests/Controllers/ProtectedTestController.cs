using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elevating.Api.IntegrationTests.Controllers;

[ApiController]
[Route("api/test-auth")]
public sealed class ProtectedTestController : ControllerBase
{
    [Authorize]
    [HttpGet("protected")]
    public IActionResult GetProtected()
    {
        return NoContent();
    }
}