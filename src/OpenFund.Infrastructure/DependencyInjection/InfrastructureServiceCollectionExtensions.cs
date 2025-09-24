namespace OpenFund.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));

        services.AddDbContext<OpenFundDbContext>(o =>
            o.UseNpgsql(config.GetConnectionString("Default")));

        services.AddScoped<IUsersRepo, UsersRepo>();
        services.AddScoped<IJwtProvider, JwtProvider>();

        return services;
    }
}
