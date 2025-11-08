using MediatR;
using Microsoft.AspNetCore.Identity;
using OpenFund.Core.CQS.Auth.Commands;

namespace OpenFund.Core.CQS.Auth.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand>
{
    private readonly UserManager<IdentityUser> _userManager;

    public RegisterUserCommandHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var registrationModel = request.Model;

        var user = await _userManager.FindByEmailAsync(registrationModel.Email);
        if (user != null) throw new NotImplementedException();

        user = new IdentityUser()
        {
            UserName = registrationModel.UserName,
            Email = registrationModel.Email,
        };
        
        var hash = _userManager.PasswordHasher.HashPassword(user, registrationModel.Password);
        user.PasswordHash = hash;
        await _userManager.CreateAsync(user);
    }
}