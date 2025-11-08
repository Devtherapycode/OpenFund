using MediatR;
using Microsoft.AspNetCore.Identity;
using OpenFund.Core.CQS.Auth.Commands;
using OpenFund.Core.DTOs;
using OpenFund.Core.Entities;
using OpenFund.Core.Interfaces.Managers;
using OpenFund.Core.Interfaces.Repositories;

namespace OpenFund.Core.CQS.Auth.Handlers;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthTokenDto>
{
    private readonly ITokenManager _tokenManager;
    private readonly UserManager<IdentityUser> _userManager;
    
    public LoginUserCommandHandler(
        ITokenManager tokenManager,
        UserManager<IdentityUser> userManager)
    {
        _tokenManager = tokenManager;
        _userManager = userManager;
    }

    public async Task<AuthTokenDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var loginModel = request.Model;
        
        var user = await _userManager.FindByEmailAsync(loginModel.Email);
        if (user == null) throw new NotImplementedException();

        var passwordMatches = await _userManager.CheckPasswordAsync(user, loginModel.Password);
        if (!passwordMatches) throw new NotImplementedException();

        var authTokenDto = await _tokenManager.CreateAuthenticationTokensAsync(
            user.Id,
            user.Email!,
            cancellationToken);
        
        return authTokenDto;
    }
}