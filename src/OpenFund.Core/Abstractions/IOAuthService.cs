using Microsoft.AspNetCore.Authentication;
using OpenFund.Core.Entities;

namespace OpenFund.Core.Abstractions;

public interface IOAuthService
{
    Task<OAuthResult> HandleCallbackAsync(
        AuthenticateResult authenticateResult,
        CancellationToken cancellationToken = default);
}

public record OAuthResult(
    bool Success,
    User? User,
    string? JwtToken,
    string? RefreshToken,
    string? ErrorMessage = null);

public record OAuthUserInfo(
    string GoogleId,
    string Email,
    string? Name,
    string? Picture,
    string? AccessToken,
    string? RefreshToken);
