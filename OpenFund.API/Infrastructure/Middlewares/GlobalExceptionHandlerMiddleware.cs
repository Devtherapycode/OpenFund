using FluentValidation;
using OpenFund.Core.Common;

namespace OpenFund.API.Infrastructure.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next = null!;
    private ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (ValidationException validationException)
        {
            var errorsDictionary = GetErrorsDictionaryFromValidationException(validationException);
            var result = Result.Failure("Request data is invalid", errors: errorsDictionary);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(result);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(
                "Unexpected exception occured: {0}\nStack Trace: {1}\nInner Exception: {2}", 
                ex.Message,
                ex.StackTrace,
                ex.InnerException?.Message);

            var response = new { message = "Internal server error" };
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(response);
        }
    }

    private Dictionary<string, ICollection<string>> GetErrorsDictionaryFromValidationException(ValidationException validationException)
    {
        var validationErrors = validationException.Errors;
        var errors = new Dictionary<string, ICollection<string>>();

        foreach (var validationResultError in validationErrors)
        {
            errors.TryGetValue(validationResultError.PropertyName, out var propertyValidationErrors);

            if (propertyValidationErrors != null)
                propertyValidationErrors.Add(validationResultError.ErrorMessage);
            else
                errors.Add(validationResultError.PropertyName, new List<string> { validationResultError.ErrorMessage });
        }

        return errors;
    }
}