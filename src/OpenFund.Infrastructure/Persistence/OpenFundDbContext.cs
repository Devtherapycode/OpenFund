namespace OpenFund.Infrastructure.Persistence;

public class OpenFundDbContext : IdentityDbContext<User,IdentityRole<Guid>,Guid>
{
    public OpenFundDbContext(DbContextOptions<OpenFundDbContext> options)
        : base(options) { }
}
