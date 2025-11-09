using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace OpenFund.Infrastructure.Auth;

public class YouTubeOAuthService(
    UserManager<User> userManager,
    IJwtProvider jwtProvider,
    ITokenStorageService tokenStorage,
    ILogger<YouTubeOAuthService> logger)
    : IOAuthService
{
    public async Task<OAuthResult> HandleCallbackAsync(
        AuthenticateResult authenticateResult,
        CancellationToken cancellationToken = default)
    {
        if (!authenticateResult.Succeeded)
        {
            return new OAuthResult(false, null, null, null, "Authentication failed");
        }

        logger.LogInformation("YouTube authentication succeeded");

        var userInfo = ExtractUserInfo(authenticateResult);
        if (userInfo == null)
        {
            return new OAuthResult(false, null, null, null, "Missing required claims");
        }

        var user = await FindOrCreateUserAsync(userInfo, cancellationToken);
        if (user == null)
        {
            logger.LogWarning("User not found for YouTube auth: {Email}", userInfo.Email);
            return new OAuthResult(false, null, null, null, "User not found. Please sign in with Google first.");
        }

        logger.LogInformation("User {Email} is now a creator (YouTube connected)", user.Email);

        var (jwtToken, expiresAt) = jwtProvider.Create(user);
        var jwtRefreshToken = jwtProvider.GenerateRefreshToken();

        await tokenStorage.CreateRefreshTokenAsync(
            user.Id,
            jwtRefreshToken,
            youtubeAccessToken: userInfo.AccessToken,
            youtubeRefreshToken: userInfo.RefreshToken,
            cancellationToken: cancellationToken);

        return new OAuthResult(true, user, jwtToken, jwtRefreshToken);
    }

    private static OAuthUserInfo? ExtractUserInfo(AuthenticateResult authenticateResult)
    {
        var claims = authenticateResult.Principal?.Claims;
        var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var picture = claims?.FirstOrDefault(c => c.Type == "picture")?.Value;

        if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
        {
            return null;
        }

        var accessToken = authenticateResult.Properties?.GetTokenValue("access_token");
        var refreshToken = authenticateResult.Properties?.GetTokenValue("refresh_token");

        return new OAuthUserInfo(googleId, email, name, picture, accessToken, refreshToken);
    }

    private async Task<User?> FindOrCreateUserAsync(OAuthUserInfo userInfo, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(userInfo.Email);

        if (user == null)
        {
            user = new User
            {
                Email = userInfo.Email,
                UserName = userInfo.Email,
                DisplayName = userInfo.Name,
                GoogleId = userInfo.GoogleId,
                Avatar = userInfo.Picture,
                EmailConfirmed = true,
                IsCreator = true,
                LastLoginAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(user);
            return createResult.Succeeded ? user : null;
        }
        else
        {
            user.GoogleId = userInfo.GoogleId;
            user.Avatar = userInfo.Picture;
            user.DisplayName = userInfo.Name ?? user.DisplayName;
            user.LastLoginAt = DateTime.UtcNow;

            await userManager.UpdateAsync(user);
        }

        return user;
    }
}