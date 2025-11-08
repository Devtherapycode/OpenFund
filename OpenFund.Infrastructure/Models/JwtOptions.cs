namespace OpenFund.Infrastructure.Models;

public sealed class JwtOptions
{
    public string Key { get; private set; }
    public int ExpirationInMinutes { get; private set; }

    public JwtOptions(string key, int expirationInMinutes)
    {
        Key = key;
        ExpirationInMinutes = expirationInMinutes;
    }
}