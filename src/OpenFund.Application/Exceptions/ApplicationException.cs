using System;
using System.Globalization;

namespace OpenFund.Application.Exceptions;

public class ApplicationException : Exception
{
    public ApplicationException() : base()
    {
    }

    protected ApplicationException(string message) : base(message)
    {
    }

    public ApplicationException(string message, params object[] args)
        : base(String.Format(CultureInfo.CurrentCulture, message, args))
    {
    }

    public static string GetExceptionMessages(Exception? e, string msg = "")
    {
        if (e == null)
            return string.Empty;

        if (msg == "")
            msg = e.Message;

        if (e.InnerException != null)
            msg += "\r\nInnerException: " + GetExceptionMessages(e.InnerException);

        return msg;
    }
}