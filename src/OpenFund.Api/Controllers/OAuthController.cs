using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using OpenFund.Core.Abstractions;
using OpenFund.Infrastructure.Auth;

namespace OpenFund.Api.Controllers;

[ApiController]
[Route("auth")]
public class OAuthController(
    IOAuthService googleOAuthService,
    YouTubeOAuthService youtubeOAuthService,
    IConfiguration configuration,
    ILogger<OAuthController> logger) : ControllerBase
{
    [HttpGet("google")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback)),
            Items =
            {
                { "scheme", GoogleDefaults.AuthenticationScheme }
            }
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback(CancellationToken ct)
    {
        logger.LogInformation("Google OAuth callback received");

        var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        var result = await googleOAuthService.HandleCallbackAsync(authenticateResult, ct);

        var frontendBaseUrl = configuration["Frontend:Url"];

        if (!result.Success)
        {
            logger.LogError("Google OAuth flow failed: {Error}", result.ErrorMessage);
            return Redirect(
                $"{frontendBaseUrl}/auth/error?message={Uri.EscapeDataString(result.ErrorMessage ?? "Authentication failed")}");
        }

        var frontendUrl =
            $"{frontendBaseUrl}/auth/success?accessToken={Uri.EscapeDataString(result.JwtToken!)}&refreshToken={Uri.EscapeDataString(result.RefreshToken!)}";
        return Redirect(frontendUrl);
    }

    [HttpGet("youtube")]
    public IActionResult YouTubeLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(YouTubeCallback)),
            Items =
            {
                { "scheme", GoogleDefaults.AuthenticationScheme }
            }
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("youtube/callback")]
    public async Task<IActionResult> YouTubeCallback(CancellationToken ct)
    {
        logger.LogInformation("YouTube OAuth callback received");

        var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        var result = await youtubeOAuthService.HandleCallbackAsync(authenticateResult, ct);

        var frontendBaseUrl = configuration["Frontend:Url"];

        if (!result.Success)
        {
            logger.LogError("YouTube OAuth flow failed: {Error}", result.ErrorMessage);
            return Redirect(
                $"{frontendBaseUrl}/auth/error?message={Uri.EscapeDataString(result.ErrorMessage ?? "Authentication failed")}");
        }

        var frontendUrl =
            $"{frontendBaseUrl}/auth/success?accessToken={Uri.EscapeDataString(result.JwtToken!)}&refreshToken={Uri.EscapeDataString(result.RefreshToken!)}";
        return Redirect(frontendUrl);
    }
}
