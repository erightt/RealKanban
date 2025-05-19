namespace KanbanApp.API.Middleware;

using KanbanApp.API.Exceptions;
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Произошла ошибка");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        return exception switch
        {
            RegistrationException => HandleRegistrationException(context, exception),
            AuthenticationException => HandleAuthException(context, exception),
            _ => HandleGenericException(context)
        };
    }

    private static Task HandleRegistrationException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return context.Response.WriteAsync(new ErrorDetails(
            StatusCodes.Status400BadRequest,
            ex.Message).ToString());
    }

    private static Task HandleAuthException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return context.Response.WriteAsync(new ErrorDetails(
            StatusCodes.Status401Unauthorized,
            ex.Message).ToString());
    }

    private static Task HandleGenericException(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return context.Response.WriteAsync(new ErrorDetails(
            StatusCodes.Status500InternalServerError,
            "Внутренняя ошибка сервера").ToString());
    }
}

public record ErrorDetails(int StatusCode, string Message)
{
    public override string ToString() => 
        System.Text.Json.JsonSerializer.Serialize(this);
}