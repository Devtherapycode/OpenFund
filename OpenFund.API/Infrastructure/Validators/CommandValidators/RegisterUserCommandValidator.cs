using FluentValidation;
using OpenFund.API.Infrastructure.Validators.DTOValidators;
using OpenFund.Core.CQS.Auth.Commands;

namespace OpenFund.API.Infrastructure.Validators.CommandValidators;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(r => r.Model).SetValidator(new UserRegistrationValidator());
    }
}