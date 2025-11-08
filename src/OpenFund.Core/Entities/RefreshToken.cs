namespace OpenFund.Core.Entities;

public class RefreshToken
{
    public string Token { get; private set; }
    public DateTime Expiration { get; private set; }

    public RefreshToken(string token, DateTime expiration)
    {
        Token = token;
        Expiration = expiration;
    }

    public bool IsExpired()
    {
            var currentDateTime = DateTime.UtcNow;
            return currentDateTime >= Expiration;
    }
}