using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFund.Core.CQS.Auth.Commands;
using OpenFund.Core.DTOs;
using OpenFund.API.Infrastructure.Extensions;
using OpenFund.Core.Common;
using OpenFund.Core.Interfaces.Managers;

namespace OpenFund.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IExternalAuthManager _externalAuthManager;
    private readonly IConfiguration _configuration;
    
    public AuthController(
        ILogger<BaseController> logger,
        IMediator mediator,
        IExternalAuthManager externalAuthManager,
        IConfiguration configuration) : base(logger, mediator)
    {
        _externalAuthManager = externalAuthManager;
        _configuration = configuration;
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

    [HttpGet("google/link")]
    public IActionResult GetGoogleLoginLink()
    {
        var redirectUri = GetGoogleRedirectUriFromHttpRequest(HttpContext.Request);
        var response = _externalAuthManager.GetInitialGmailAuthenticationLinkAsync(redirectUri);
        return Result<string>.Success(response).ToActionResult();
    }
    
    [HttpPost("google/login")]
    public async Task<IActionResult> LoginWithGmailAsync([FromQuery] string code)
    {
        var redirectUri = GetGoogleRedirectUriFromHttpRequest(HttpContext.Request);
        var command = new LoginUserWithExternalProviderCommand(redirectUri, code);
        var response = await _mediator.Send(command);
        return response.ToActionResult();
    }

    // TEMP: while the app is not live on the cloud.
    private string GetGoogleRedirectUriFromHttpRequest(HttpRequest httpRequest)
    {
        var tempRedirectUrl = _configuration["Google:RedirectUri"]!;
        return tempRedirectUrl;
    }
}