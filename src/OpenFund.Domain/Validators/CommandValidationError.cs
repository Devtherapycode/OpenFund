namespace OpenFund.Domain.Validators;

public class CommandValidationError
{
    public required string ErrorMessage { get; set; }
    public required string ErrorCode { get; set; }
    public required string PropertyName { get; set; }
}