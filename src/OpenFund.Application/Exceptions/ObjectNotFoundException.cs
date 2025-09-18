namespace OpenFund.Application.Exceptions;

public class ObjectNotFoundException : ApplicationException
{
    public ObjectNotFoundException(string entityName, string propertyName, object propertyValue) : base(
        $"{entityName} with the {propertyName}: {propertyValue.ToString()} not found.")
    {
    }

    public ObjectNotFoundException(string message) : base(message)
    {
    }
}