using System.Net;
using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OpenFund.Core.Common;
using OpenFund.Core.CQS.Auth.Commands;
using OpenFund.Core.DTOs;
using OpenFund.Core.Interfaces.Managers;

namespace OpenFund.Core.CQS.Auth.Handlers;

public class LoginUserWithExternalProviderCommandHandler : IRequestHandler<LoginUserWithExternalProviderCommand, Result<AuthTokenDto>>
{
    private readonly IExternalAuthManager _externalAuthManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenManager _tokenManager;

    public LoginUserWithExternalProviderCommandHandler(
        IExternalAuthManager externalAuthManager,
        UserManager<IdentityUser> userManager,
        ITokenManager tokenManager)
    {
        _externalAuthManager = externalAuthManager;
        _userManager = userManager;
        _tokenManager = tokenManager;
    }

    public async Task<Result<AuthTokenDto>> Handle(LoginUserWithExternalProviderCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code;
        var redirectUri = request.RedirectUri;

        var googleUserInfo = await _externalAuthManager.AuthenticateByGmailAsync(redirectUri, code);

        var user = await _userManager.FindByEmailAsync(googleUserInfo.Email);
        if (user == null)
        {
            await CreateUserIfNotExists(googleUserInfo);
            user = await _userManager.FindByEmailAsync(googleUserInfo.Email);
        }
        
        var authTokenDto = await _tokenManager.CreateAuthenticationTokensAsync(user.Id, user.Email!, cancellationToken);
        return Result<AuthTokenDto>.Success(authTokenDto);
    }

    private async Task CreateUserIfNotExists(GoogleUserInfoDto googleUserInfoDto)
    {
        var username = googleUserInfoDto.Email.Split("@")[0]!;
        
        var randomBytes = new byte[32];
        RandomNumberGenerator.Fill(randomBytes);
        var randomPassword = Convert.ToBase64String(randomBytes);
        
        var user = new IdentityUser()
        {
            UserName = username,
            Email = googleUserInfoDto.Email,
            EmailConfirmed = true,
        };
        
        _userManager.PasswordHasher.HashPassword(user, randomPassword);
        await _userManager.CreateAsync(user, randomPassword);
    }
}