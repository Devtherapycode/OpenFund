using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using OpenFund.Core.Abstractions;

namespace OpenFund.Api.Endpoints;

public static class OAuthEndpoints
{
    public static IEndpointRouteBuilder MapOAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth");

        auth.MapGet("/google", (HttpContext context, ILogger<Program> logger) =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/auth/google/callback",
                Items =
                {
                    { "scheme", GoogleDefaults.AuthenticationScheme }
                }
            };

            return Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]);
        });

        auth.MapGet("/google/callback", async (
            HttpContext context,
            IOAuthService googleOAuthService,
            IConfiguration configuration,
            ILogger<Program> logger,
            CancellationToken ct) =>
        {
            logger.LogInformation("Google OAuth callback received");

            var authenticateResult = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            var result = await googleOAuthService.HandleCallbackAsync(authenticateResult, ct);

            var frontendBaseUrl = configuration["Frontend:Url"];

            if (!result.Success)
            {
                logger.LogError("Google OAuth flow failed: {Error}", result.ErrorMessage);
                return Results.Redirect(
                    $"{frontendBaseUrl}/auth/error?message={Uri.EscapeDataString(result.ErrorMessage ?? "Authentication failed")}");
            }

            var frontendUrl =
                $"{frontendBaseUrl}/auth/success?accessToken={Uri.EscapeDataString(result.JwtToken!)}&refreshToken={Uri.EscapeDataString(result.RefreshToken!)}";
            return Results.Redirect(frontendUrl);
        });

        auth.MapGet("/youtube", (HttpContext context) =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/auth/youtube/callback",
                Items =
                {
                    { "scheme", GoogleDefaults.AuthenticationScheme },
                    // {
                    //     "scope",
                    //     "https://www.googleapis.com/auth/youtube.readonly"
                    // }
                }
            };

            return Results.Challenge(properties, new[] { GoogleDefaults.AuthenticationScheme });
        });

        // YouTube OAuth - Callback handler
        auth.MapGet("/youtube/callback", async (
            HttpContext context,
            YouTubeOAuthService youtubeOAuthService,
            IConfiguration configuration,
            ILogger<Program> logger,
            CancellationToken ct) =>
        {
            logger.LogInformation("YouTube OAuth callback received");

            // Authenticate with Google
            var authenticateResult = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            var result = await youtubeOAuthService.HandleCallbackAsync(authenticateResult, ct);

            var frontendBaseUrl = configuration["Frontend:Url"];

            if (!result.Success)
            {
                logger.LogError("YouTube OAuth flow failed: {Error}", result.ErrorMessage);
                return Results.Redirect(
                    $"{frontendBaseUrl}/auth/error?message={Uri.EscapeDataString(result.ErrorMessage ?? "Authentication failed")}");
            }

            var frontendUrl =
                $"{frontendBaseUrl}/auth/success?accessToken={Uri.EscapeDataString(result.JwtToken!)}&refreshToken={Uri.EscapeDataString(result.RefreshToken!)}";
            return Results.Redirect(frontendUrl);
        });

        return app;
    }
}