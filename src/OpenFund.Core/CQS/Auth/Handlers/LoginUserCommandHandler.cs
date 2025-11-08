using MediatR;
using Microsoft.AspNetCore.Identity;
using OpenFund.Core.Common;
using OpenFund.Core.CQS.Auth.Commands;
using OpenFund.Core.DTOs;
using OpenFund.Core.Interfaces.Managers;

namespace OpenFund.Core.CQS.Auth.Handlers;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<AuthTokenDto>>
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

    public async Task<Result<AuthTokenDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var loginModel = request.Model;
        
        var user = await _userManager.FindByEmailAsync(loginModel.Email);
        if (user == null) 
            return Result<AuthTokenDto>.Failure("Invalid Credentials");

        var passwordMatches = await _userManager.CheckPasswordAsync(user, loginModel.Password);
        if (!passwordMatches) 
            return Result<AuthTokenDto>.Failure("Invalid credentials");

        var authTokenDto = await _tokenManager.CreateAuthenticationTokensAsync(
            user.Id,
            user.Email!,
            cancellationToken);
        
        return Result<AuthTokenDto>.Success(authTokenDto);
    }
}