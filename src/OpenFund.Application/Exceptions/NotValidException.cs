namespace OpenFund.Application.Exceptions;

public class NotValidException : ApplicationException
{
    public NotValidException(string propertyName) : base($"{propertyName} is not valid.")
    {
    }
}