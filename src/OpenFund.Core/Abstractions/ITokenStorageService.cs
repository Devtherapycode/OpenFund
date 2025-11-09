namespace OpenFund.Core.Abstractions;

public interface ITokenStorageService
{
    Task<RefreshToken> CreateRefreshTokenAsync(
        Guid userId,
        string jwtRefreshToken,
        string? googleAccessToken = null,
        string? googleRefreshToken = null,
        string? youtubeAccessToken = null,
        string? youtubeRefreshToken = null,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);

    Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default);

    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<string> EncryptTokenAsync(string token);

    Task<string> DecryptTokenAsync(string encryptedToken);
}
