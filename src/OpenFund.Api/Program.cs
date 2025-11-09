var builder = WebApplication.CreateBuilder(args);

// Configure detailed console logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore.Authorization", LogLevel.Debug);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:63342",
                "https://localhost:63342",
                "http://localhost:5005",
                "https://localhost:5005",
                "http://localhost:8000",
                "https://localhost:8000"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddCoreServices();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure JWT options
var jwtConfig = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtConfig["Key"] ?? throw new InvalidOperationException("JWT Key not configured");

// Add Authentication - JWT as default, Cookies only for OAuth flow
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    })
    .AddCookie("TempOAuth", options =>
    {
        // Minimal cookie for OAuth flow state only
        options.Cookie.Name = "OpenFund.OAuth.State";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(15); // Short-lived for OAuth flow only
    })
    .AddGoogle(options =>
    {
        var googleAuth = builder.Configuration.GetSection("GoogleAuth");
        options.ClientId = googleAuth["ClientId"] ??
                           throw new InvalidOperationException("Google ClientId not configured");
        options.ClientSecret = googleAuth["ClientSecret"] ??
                               throw new InvalidOperationException("Google ClientSecret not configured");

        // Save tokens for later use
        options.SaveTokens = true;
        options.SignInScheme = "TempOAuth";
        options.AccessType = "offline";

        // Request profile and email (these are allowed by default)
        options.Scope.Clear(); // Clear defaults
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        // Log OAuth events
        options.Events.OnCreatingTicket = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("OAuth ticket created for user: {Email}",
                context.Principal?.FindFirst("email")?.Value);
            return Task.CompletedTask;
        };

        options.Events.OnRemoteFailure = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("OAuth failed: {Error}", context.Failure?.Message);
            return Task.CompletedTask;
        };
    });

// Add Identity
builder.Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<OpenFundDbContext>()
    .AddSignInManager<SignInManager<User>>()
    .AddUserManager<UserManager<User>>()
    .AddDefaultTokenProviders();

// Add Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Request logging middleware
app.Use(async (context, next) =>
{
    var requestLogger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    requestLogger.LogInformation("{Method} {Path}", context.Request.Method, context.Request.Path);

    await next();

    requestLogger.LogInformation("{Method} {Path} -> {StatusCode}",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode);
});

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapOAuthEndpoints();

app.Run();