using System.Security.Claims;

namespace OpenFund.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth");

        auth.MapPost("/register", async (RegisterRequest req, IAuthService authSvc, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password) ||
                string.IsNullOrWhiteSpace(req.DisplayName))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["email"] = ["Required"],
                    ["password"] = ["Required"],
                    ["displayName"] = ["Required"]
                });
            }

            try
            {
                var res = await authSvc.RegisterAsync(req, ct);
                return Results.Ok(res);
            }
            catch (InvalidOperationException e)
            {
                return Results.Conflict(new { error = e.Message });
            }
        });

        auth.MapPost("/login", async (LoginRequest req, IAuthService authSvc, CancellationToken ct) =>
        {
            try
            {
                var res = await authSvc.LoginAsync(req, ct);
                return Results.Ok(res);
            }
            catch
            {
                return Results.Unauthorized();
            }
        });


        app.MapGet("/me", (HttpContext ctx) =>
        {
            var sub = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = ctx.User.FindFirst(ClaimTypes.Email)?.Value;
            var name = ctx.User.FindFirst(ClaimTypes.Name)?.Value;

            if (sub is null) return Results.Unauthorized();

            return Results.Ok(new MeResponse(Guid.Parse(sub), email ?? "", name ?? "", DateTime.UtcNow));
        })
        .RequireAuthorization();

        return app;
    }
}
