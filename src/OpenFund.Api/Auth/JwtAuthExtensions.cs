namespace OpenFund.Api.Auth;

public static class JwtAuthExtensions
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config, bool addSwaggerWithJwt = true)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

        var jwt = config.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

        var keyBytes = Encoding.UTF8.GetBytes(jwt.Key);
        var signingKey = new SymmetricSecurityKey(keyBytes);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = signingKey
                };
            });

        services.AddAuthorization();

        if (addSwaggerWithJwt)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OpenFund API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Example:"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        return services;
    }
}
