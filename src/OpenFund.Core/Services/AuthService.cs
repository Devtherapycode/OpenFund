namespace OpenFund.Core.Services;

internal sealed class AuthService(IUsersRepo users, IJwtProvider jwt, IPasswordHasher<User> hasher, UserManager<User> userManager)
    : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        var existing = await users.GetByEmailAsync(req.Email, ct);
        if (existing is not null) throw new InvalidOperationException("Email already registered.");

        var user = new User { Email = req.Email.Trim().ToLowerInvariant(), UserName = req.DisplayName };
        
        var createUser = await userManager.CreateAsync(user, req.Password);
        
        if (!createUser.Succeeded)
        {
            var errorMessage = string.Join("\n", createUser.Errors.Select(e => e.Description));
           
            throw new AggregateException(errorMessage); 
        }
        
        var (token, exp) = jwt.Create(user);
        return new AuthResponse(token, exp);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(req.Email) ??
            throw new UnauthorizedAccessException("Invalid credentials.");


        var checkPassword = await userManager.CheckPasswordAsync(user, req.Password);

        if (checkPassword == false)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var (token, exp) = jwt.Create(user);
        return new AuthResponse(token, exp);
    }
}
