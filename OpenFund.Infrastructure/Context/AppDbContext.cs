using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenFund.Core.Entities;
using OpenFund.Infrastructure.Options;

namespace OpenFund.Infrastructure.Context;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    private readonly DbOptions _dbOptions;
    
    public DbSet<RefreshToken> RefreshToken { get; private set; }
    
    public AppDbContext(
        DbContextOptions<AppDbContext> appDbContextOptions,
        IOptions<DbOptions> dbOptions) : base(appDbContextOptions)
    {
        _dbOptions = dbOptions.Value;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_dbOptions.ConnectionString);
    }
}