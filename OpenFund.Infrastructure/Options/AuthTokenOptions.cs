namespace OpenFund.Infrastructure.Options;

public sealed class AuthTokenOptions
{
    public required string Key { get; init; }
    public required int ExpirationInMinutes { get; init; }
    public required int RefreshTokenExpirationInMinutes { get; init; }
}