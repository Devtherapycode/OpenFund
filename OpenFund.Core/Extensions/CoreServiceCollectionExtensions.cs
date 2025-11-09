using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace OpenFund.Core.Extensions;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection RegisterCoreServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        
        return services;
    }
}