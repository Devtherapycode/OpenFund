using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenFund.Core.DTOs;
using OpenFund.Core.Entities;
using OpenFund.Core.Interfaces.Managers;
using OpenFund.Core.Interfaces.Repositories;
using OpenFund.Infrastructure.Options;

namespace OpenFund.Infrastructure.Managers;

public class TokenManager : ITokenManager
{
    private readonly AuthTokenOptions _authTokenOptions;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public TokenManager(IOptions<AuthTokenOptions> jwtConfiguration, IRefreshTokenRepository refreshTokenRepository)
    {
        _authTokenOptions = jwtConfiguration.Value;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<AuthTokenDto> CreateAuthenticationTokensAsync(string userId, string email, CancellationToken cancellationToken)
    {
        var jwt = GenerateJwtToken(userId, email);
        var refreshToken = GenerateRefreshToken();

        var authTokenDto = new AuthTokenDto(jwt, refreshToken);

        var refreshTokenEntity = new RefreshToken(
            refreshToken,
            DateTime.UtcNow.AddMinutes(_authTokenOptions.RefreshTokenExpirationInMinutes));

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        
        return authTokenDto;
    }
    
    private string GenerateJwtToken(string userId, string email)
    {
        var key = _authTokenOptions.Key;
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var symmetricSecurityKey = new SymmetricSecurityKey(keyBytes);
        
        var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var claims = GetClaims(userId, email);
        var expiration = DateTime.Now.AddMinutes(_authTokenOptions.ExpirationInMinutes);
        
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