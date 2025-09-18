using OpenFund.Domain.Validators;

namespace OpenFund.Domain.Exceptions;

public class CommandValidationException : DomainException
{
    public CommandValidationException(IEnumerable<CommandValidationError> messages) : base(
        messages.Select(x => x.ErrorMessage))
    {
    }
}