namespace OpenFund.Infrastructure.Persistence;

public sealed class OpenFundDbContext(DbContextOptions<OpenFundDbContext> options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.GoogleId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.GoogleId).HasMaxLength(255);
            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.IsCreator).HasDefaultValue(false);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Token);
            entity.HasIndex(e => e.ExpiresAt);

            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.Property(e => e.GoogleAccessToken).HasMaxLength(1000);
            entity.Property(e => e.GoogleRefreshToken).HasMaxLength(1000);
            entity.Property(e => e.YoutubeAccessToken).HasMaxLength(1000);
            entity.Property(e => e.YoutubeRefreshToken).HasMaxLength(1000);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
