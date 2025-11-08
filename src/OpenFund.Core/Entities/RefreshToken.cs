namespace OpenFund.Core.Entities;

public class RefreshToken
{
    public string Id { get; private set; }
    public string Token { get; private set; }
    public DateTime Expiration { get; private set; }

    public RefreshToken(string id, string token, DateTime expiration)
    {
        Id = id;
        Token = token;
        Expiration = expiration;
    }

    public bool IsExpired()
    {
            var currentDateTime = DateTime.UtcNow;
            return currentDateTime >= Expiration;
    }
}