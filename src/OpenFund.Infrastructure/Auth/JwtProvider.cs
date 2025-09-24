namespace OpenFund.Infrastructure.Auth;

internal sealed class JwtProvider(IOptions<JwtOptions> opts) : IJwtProvider
{
    public (string token, DateTime expiresAtUtc) Create(User user)
    {
        var value = opts.Value;
        var expires = DateTime.UtcNow.AddMinutes(value.ExpiryMinutes);

        var claims = new[]
        {
            new Claim("id", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("name", user.DisplayName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(value.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: value.Issuer,
            audience: value.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }
}
