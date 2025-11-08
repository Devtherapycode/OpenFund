using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenFund.Infrastructure.Context;
using OpenFund.Infrastructure.Options;
using OpenFund.Infrastructure.Utilities;

namespace OpenFund.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        #region Options
        
        services.Configure<DbOptions>(configuration.GetSection(nameof(DbOptions)));
        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));
        
        #endregion

        #region Database

        services.AddDbContext<AppDbContext>();

        #endregion

        #region Identity

        services.AddIdentity<IdentityUser, IdentityRole>(opt =>
            {
                opt.Password.RequireDigit = true;
                opt.Password.RequiredUniqueChars = 1;
                opt.Password.RequiredLength = 8;
                opt.Password.RequireUppercase = true;
                opt.Password.RequireLowercase = true;
                
                opt.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        #endregion
        
        #region Utilities

        services.AddScoped<JwtUtility>();
        
        #endregion
        
        return services;
    }
}