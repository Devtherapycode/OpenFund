using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFund.Core.CQS.Auth.Commands;
using OpenFund.Core.DTOs;
using OpenFund.API.Infrastructure.Extensions;

namespace OpenFund.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    public AuthController(
        ILogger<BaseController> logger,
        IMediator mediator) : base(logger, mediator) { }

    /// <summary>
    /// User login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(UserLoginDto userLoginDto)
    {
        var command = new LoginUserCommand(userLoginDto);
        var response = await _mediator.Send(command);
        return response.ToActionResult();
    }

    /// <summary>
    /// User registration
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> RegisterAsync(UserRegistrationDto userRegistrationDto)
    {
        var command = new RegisterUserCommand(userRegistrationDto);
        var response = await _mediator.Send(command);
        return response.ToActionResult();
    }
}