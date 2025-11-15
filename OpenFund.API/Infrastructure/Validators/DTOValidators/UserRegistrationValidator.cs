using FluentValidation;
using OpenFund.Core.DTOs;

namespace OpenFund.API.Infrastructure.Validators.DTOValidators;

public class UserRegistrationValidator : AbstractValidator<UserRegistrationDto>
{
    public UserRegistrationValidator()
    {
        RuleFor(u => u.UserName)
            .NotEmpty()
            .MinimumLength(4)
            .MaximumLength(50);

        RuleFor(u => u.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);

        RuleFor(u => u.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(50)
            .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,}$\n");
    }
}