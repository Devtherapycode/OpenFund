namespace OpenFund.Infrastructure.Persistence;

internal sealed class OpenFundDbContext(DbContextOptions<OpenFundDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).IsRequired();
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.DisplayName).IsRequired();
        });
    }
}
