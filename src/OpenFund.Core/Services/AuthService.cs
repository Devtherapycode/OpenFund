namespace OpenFund.Core.Services;

internal sealed class AuthService(IJwtProvider jwt, UserManager<User> userManager)
    : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var user = new User { Email = request.Email.Trim().ToLowerInvariant(), UserName = request.DisplayName };
        
        var createUser = await userManager.CreateAsync(user, request.Password);
        
        if (!createUser.Succeeded)
        {
            var errorMessage = string.Join(Environment.NewLine, createUser.Errors.Select(e => e.Description));
           
            throw new InvalidOperationException(errorMessage); 
        }
        
        var (token, exp) = jwt.Create(user);
        return new AuthResponse(token, exp);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email) ??
            throw new UnauthorizedAccessException("Invalid credentials.");
        
        var checkPassword = await userManager.CheckPasswordAsync(user, request.Password);

        if (checkPassword == false)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var (token, exp) = jwt.Create(user);
        return new AuthResponse(token, exp);
    }
}
