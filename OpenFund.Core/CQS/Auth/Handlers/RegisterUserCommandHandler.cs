using System.Net;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OpenFund.Core.Common;
using OpenFund.Core.CQS.Auth.Commands;

namespace OpenFund.Core.CQS.Auth.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result>
{
    private readonly UserManager<IdentityUser> _userManager;

    public RegisterUserCommandHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var registrationModel = request.Model;

        var user = await _userManager.FindByEmailAsync(registrationModel.Email);
        if (user != null)
            return Result.Failure("Invalid credentials");
        
        var newUser = new IdentityUser()
        {
            UserName = registrationModel.UserName,
            Email = registrationModel.Email,
        };
        
        var hash = _userManager.PasswordHasher.HashPassword(user, registrationModel.Password);
        user.PasswordHash = hash;
        await _userManager.CreateAsync(user);

        return Result.Success((int)HttpStatusCode.Created);
    }
}