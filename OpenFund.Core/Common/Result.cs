using System.Net;

namespace OpenFund.Core.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T Data { get; }
    public string Message { get; }
    public Dictionary<string, string[]>? Errors { get; }
    public int StatusCode { get; }

    private Result(bool isSuccess, T data, string message, Dictionary<string, string[]>? errors, int statusCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        Message = message;
        Errors = errors;
        StatusCode = statusCode;
    }

    public static Result<T> Success(T data, int statusCode = (int)HttpStatusCode.OK)
    {
        return new Result<T>(true, data, string.Empty, null, statusCode);
    }

    public static Result<T> Failure(string message, int statusCode = (int)HttpStatusCode.BadRequest)
    {
        return new Result<T>(false, default!, message, null, statusCode);
    }

    public static Result<T> Failure(string message, Dictionary<string, string[]> errors, int statusCode = (int)HttpStatusCode.BadRequest)
    {
        return new Result<T>(false, default!, message, errors, statusCode);
    }

    public static Result<T> NotFound(string message = "Resource not found")
    {
        return new Result<T>(false, default!, message, null, (int)HttpStatusCode.NotFound);
    }

    public static Result<T> Unauthorized(string message = "Unauthorized access")
    {
        return new Result<T>(false, default!, message, null, (int)HttpStatusCode.Unauthorized);
    }

    public static Result<T> Forbidden(string message = "Access forbidden")
    {
        return new Result<T>(false, default!, message, null, (int)HttpStatusCode.Forbidden);
    }

    public static Result<T> Conflict(string message = "Resource conflict")
    {
        return new Result<T>(false, default!, message, null, (int)HttpStatusCode.Conflict);
    }
}

public class Result
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public Dictionary<string, string[]>? Errors { get; }
    public int StatusCode { get; }

    private Result(bool isSuccess, string message, Dictionary<string, string[]>? errors, int statusCode)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors;
        StatusCode = statusCode;
    }

    public static Result Success(int statusCode = (int)HttpStatusCode.OK)
    {
        return new Result(true, string.Empty, null, statusCode);
    }

    public static Result Failure(string message, int statusCode = (int)HttpStatusCode.BadRequest)
    {
        return new Result(false, message, null, statusCode);
    }

    public static Result Failure(string message, Dictionary<string, string[]> errors, int statusCode = (int)HttpStatusCode.BadRequest)
    {
        return new Result(false, message, errors, statusCode);
    }

    public static Result NotFound(string message = "Resource not found")
    {
        return new Result(false, message, null, (int)HttpStatusCode.NotFound);
    }

    public static Result Unauthorized(string message = "Unauthorized access")
    {
        return new Result(false, message, null,  (int)HttpStatusCode.Unauthorized);
    }

    public static Result Forbidden(string message = "Access forbidden")
    {
        return new Result(false, message, null, (int)HttpStatusCode.Forbidden);
    }

    public static Result Conflict(string message = "Resource conflict")
    {
        return new Result(false, message, null, (int)HttpStatusCode.Conflict);
    }
}