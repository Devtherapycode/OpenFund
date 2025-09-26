namespace OpenFund.Core.Entities;

public class User : IdentityUser<Guid>
{
    
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
}
