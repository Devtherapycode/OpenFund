namespace OpenFund.Core.Services;

internal sealed class AuthService(IUsersRepo users, IJwtProvider jwt, IPasswordHasher<User> hasher)
    : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        var existing = await users.GetByEmailAsync(req.Email, ct);
        if (existing is not null) throw new InvalidOperationException("Email already registered.");

        var user = new User { Email = req.Email.Trim().ToLowerInvariant(), DisplayName = req.DisplayName };
        user.PasswordHash = hasher.HashPassword(user, req.Password);

        var saved = await users.AddAsync(user, ct);

        var (token, exp) = jwt.Create(saved);
        return new AuthResponse(token, exp);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(req.Email.Trim().ToLowerInvariant(), ct)
                   ?? throw new UnauthorizedAccessException("Invalid credentials.");

        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (result is PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var (token, exp) = jwt.Create(user);
        return new AuthResponse(token, exp);
    }
}
