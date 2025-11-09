namespace OpenFund.Core.Entities;

public class User : IdentityUser<Guid>
{
    public string? GoogleId { get; set; }
    public string? Avatar { get; set; }
    public string? DisplayName { get; set; }
    public bool IsCreator { get; set; }
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation property
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}