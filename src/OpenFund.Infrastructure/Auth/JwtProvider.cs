namespace OpenFund.Infrastructure.Auth;

internal sealed class JwtProvider(IOptions<JwtOptions> opts) : IJwtProvider
{
    public (string token, DateTime expiresAtUtc) Create(User user)
    {
        var value = opts.Value;
        var expires = DateTime.UtcNow.AddMinutes(value.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName)
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
