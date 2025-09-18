namespace OpenFund.Domain.Exceptions;

public class DomainException : Exception
{
    protected DomainException(IEnumerable<string> messages) : base(string.Join("," + Environment.NewLine + " ",
        messages))
    {
    }
}