using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenFund.Core.Interfaces.Managers;
using OpenFund.Core.Interfaces.Repositories;
using OpenFund.Infrastructure.Context;
using OpenFund.Infrastructure.Managers;
using OpenFund.Infrastructure.Options;
using OpenFund.Infrastructure.Repositories;

namespace OpenFund.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        #region Options
        
        services.Configure<DbOptions>(configuration.GetSection(nameof(DbOptions)));
        services.Configure<AuthTokenOptions>(configuration.GetSection(nameof(AuthTokenOptions)));
        services.Configure<GoogleOptions>(configuration.GetSection("Google"));
        
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
        
        #region Managers

        services.AddScoped<ITokenManager, TokenManager>();
        services.AddScoped<IExternalAuthProviderManager, ExternalAuthProviderManager>();
        
        #endregion

        #region Repositories

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        #endregion
        
        return services;
    }

    #region HttpClient Factory

    public static IServiceCollection AddGoogleAuthenticationHttpClient(this IServiceCollection services,
        IConfiguration configuration)
    {
        var tokenRetrievalAddress = configuration["Google:TokenRetrievalAddress"]!;

        services.AddHttpClient("Google", client =>
        {
            client.BaseAddress = new Uri(tokenRetrievalAddress);
            client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
        });

        return services;
    }

    #endregion
}