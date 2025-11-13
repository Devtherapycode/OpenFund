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
}