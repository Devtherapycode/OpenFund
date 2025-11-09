using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OpenFund.Api.Controllers;

[ApiController]
[Authorize]
public class MeController : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetMe()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value;

        if (sub is null)
        {
            return Unauthorized();
        }

        return Ok(new MeResponse(Guid.Parse(sub), email ?? "", name ?? "", DateTime.UtcNow));
    }
}
