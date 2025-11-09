namespace OpenFund.Core.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty; // Encrypted refresh token
    public string? GoogleAccessToken { get; set; } // Encrypted Google access token
    public string? GoogleRefreshToken { get; set; } // Encrypted Google refresh token
    public string? YoutubeAccessToken { get; set; } // Encrypted YouTube access token
    public string? YoutubeRefreshToken { get; set; } // Encrypted YouTube refresh token
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}
