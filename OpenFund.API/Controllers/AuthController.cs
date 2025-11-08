using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OpenFund.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    public AuthController(
        ILogger<BaseController> logger,
        IMediator mediator) : base(logger, mediator) { }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
}