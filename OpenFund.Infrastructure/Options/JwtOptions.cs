namespace OpenFund.Infrastructure.Options;

public sealed class JwtOptions
{
    public required string Key { get; init; }
    public required int ExpirationInMinutes { get; init; }
}