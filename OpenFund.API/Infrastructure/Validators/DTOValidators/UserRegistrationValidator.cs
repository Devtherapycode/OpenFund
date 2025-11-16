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
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$")
            .WithMessage(
                "Password must contain at least one lowercase letter, one uppercase letter, one digit, and be at least 8 characters long.")
            .MaximumLength(50);
    }
}