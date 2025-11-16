using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenFund.Core.CQS.Auth.Commands;
using OpenFund.Core.DTOs;
using OpenFund.API.Infrastructure.Extensions;
using OpenFund.API.Models;
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
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
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
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)] 
    public async Task<IActionResult> RegisterAsync(UserRegistrationDto userRegistrationDto)
    {
        var command = new RegisterUserCommand(userRegistrationDto);
        var response = await _mediator.Send(command);
        return response.ToActionResult();
    }
    
    /// <summary>
    ///  Retrieve Google sign-in linkup to redirect user on
    /// </summary>
    /// <returns></returns>
    [HttpGet("google/link")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public IActionResult GetGoogleLoginLink()
    {
        var redirectUri = GetGoogleRedirectUriFromHttpRequest(HttpContext.Request);
        var response = _externalAuthManager.GetInitialGmailAuthenticationLinkAsync(redirectUri);
        return Result<string>.Success(response).ToActionResult();
    }
   
    /// <summary>
    /// Login with Google using a code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpPost("google/login")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenDto>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)] 
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