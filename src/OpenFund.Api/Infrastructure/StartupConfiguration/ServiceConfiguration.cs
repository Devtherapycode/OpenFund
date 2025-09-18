using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenFund.Api.Filters;
using OpenFund.Api.Infrastructure.Options;
using OpenFund.Application;
using OpenFund.Infrastructure;
using OpenFund.Persistence;
using OpenFund.Persistence.Database;

namespace OpenFund.Api.Infrastructure.StartupConfiguration;

public static class ServiceConfiguration
{
    public static WebApplicationBuilder ConfigureService(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        IConfiguration configuration = builder.Configuration;
        var environment = builder.Environment;

        services.AddControllers()
            .AddFluentValidation(config => config.RegisterValidatorsFromAssemblyContaining<Ref>());

        services.AddPersistence(configuration);
        services.AddApplication(configuration);
        services.AddInfrastructure(configuration);

        #region Health checks

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>();

        #endregion

        services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();

        services.AddVersioning();
        services.AddSwagger();

        services.AddHealthChecks();

        services.AddCors(configuration, environment);

        services.AddLogging();

        return builder;
    }

    #region Versioning

    private static IServiceCollection AddVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(setup =>
        {
            setup.DefaultApiVersion = new ApiVersion(1, 0);
            setup.AssumeDefaultVersionWhenUnspecified = true;
            setup.ReportApiVersions = true;
        });

        services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    #endregion

    #region Swagger

    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.ConfigureOptions<ConfigureSwaggerOptions>()
            .AddSwaggerGen(swagger =>
            {
                // Set the comments path for the Swagger JSON and UI.
                // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                // swagger.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                swagger.DocumentFilter<HealthChecksFilter>();
            });

        return services;
    }

    #endregion

    #region Cors

    private static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policyBuilder =>
                {
                    string? allowedOrigins = configuration.GetValue<string>("CorsAllowedOrigins");
                    if (string.IsNullOrEmpty(allowedOrigins) || (allowedOrigins == "*" && environment.IsDevelopment()))
                    {
                        policyBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    }
                    else
                    {
                        policyBuilder.WithOrigins(allowedOrigins.Split(";"))
                            .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    }
                });
        });

        return services;
    }

    #endregion
}