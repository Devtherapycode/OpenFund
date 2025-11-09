using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFund.Core.CQS.Auth.Commands;
using OpenFund.Core.DTOs;
using OpenFund.API.Infrastructure.Extensions;
using OpenFund.Core.Interfaces.Managers;

namespace OpenFund.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IExternalAuthProviderManager _externalAuthProviderManager;

    public AuthController(
        ILogger<BaseController> logger,
        IMediator mediator,
        IExternalAuthProviderManager externalAuthProviderManager) : base(logger, mediator)
    {
        _externalAuthProviderManager = externalAuthProviderManager;
    }

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

    [HttpPost("google/callback")]
    public async Task<IActionResult> LoginWithGmailAsync([FromQuery] string code)
    {
        var redirectUri = GetGoogleRedirectUriFromHttpRequest(HttpContext.Request);

        var response = await _externalAuthProviderManager.AuthenticateByGmail(redirectUri, code);
        return Ok(response);
    }

    private string GetGoogleRedirectUriFromHttpRequest(HttpRequest httpRequest)
    {
        var scheme = httpRequest.Scheme;
        var host = httpRequest.Host.Value;
        var path = "auth/google/callback";

        return $"{scheme}://{host}/{path}";
    }
}