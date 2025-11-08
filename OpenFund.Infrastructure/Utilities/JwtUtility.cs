using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenFund.Core.DTOs;
using OpenFund.Infrastructure.Options;

namespace OpenFund.Infrastructure.Utilities;

public class JwtUtility
{
    private readonly JwtOptions _jwtOptions;

    public JwtUtility(IOptions<JwtOptions> jwtConfiguration)
    {
        _jwtOptions = jwtConfiguration.Value;
    }

    public AuthTokenDto GenerateAuthenticationTokens(UserDto userDto)
    {
        var jwt = GenerateJwtToken(userDto);
        var refreshToken = GenerateRefreshToken();

        var authTokenDto = new AuthTokenDto(jwt, refreshToken);

        return authTokenDto;
    }
    
    private string GenerateJwtToken(UserDto userDto)
    {
        var key = _jwtOptions.Key;
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var symmetricSecurityKey = new SymmetricSecurityKey(keyBytes);
        
        var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var claims = GetClaims(userDto.Id, userDto.Email!);
        var expiration = DateTime.Now.AddMinutes(_jwtOptions.ExpirationInMinutes);
        
        var token = new JwtSecurityToken(
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        RandomNumberGenerator.Fill(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private Claim[] GetClaims(string id, string email)
    {
        var claims = new Claim[]
        {
            new Claim(JwtRegisteredClaimNames.NameId, id),
            new Claim(JwtRegisteredClaimNames.Email, email),
        };

        return claims;
    }
}