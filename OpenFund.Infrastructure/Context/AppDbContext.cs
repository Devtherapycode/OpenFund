using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenFund.Infrastructure.Options;

namespace OpenFund.Infrastructure.Context;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    private readonly DbOptions _dbOptions;

    public AppDbContext(
        DbContextOptions<AppDbContext> appDbContextOptions,
        IOptions<DbOptions> dbOptions) : base(appDbContextOptions)
    {
        _dbOptions = dbOptions.Value;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_dbOptions.ConnectionString);
    }
}