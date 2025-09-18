namespace OpenFund.Application.Exceptions;

public class ObjectAlreadyExistException : ApplicationException
{
    public ObjectAlreadyExistException(string objectName, string propertyName, string propertyValue) : base(
        $"{objectName} with {propertyName} - {propertyValue} already exist.")
    {
    }
}