namespace OpenFund.Domain.Validators;

[AttributeUsage(AttributeTargets.Class)]
public class CommandValidationAttribute : Attribute
{
    public required Type ValidatorType { get; set; }
    public required string ValidatorName { get; set; }

    public CommandValidationAttribute(Type validatorType)
    {
        ValidatorType = validatorType;
        ValidatorName = validatorType.Name;
    }
}