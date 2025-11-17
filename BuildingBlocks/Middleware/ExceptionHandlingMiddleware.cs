namespace BuildingBlocks.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        _logger.LogWarning("Validation error: {Errors}", string.Join(", ", exception.Errors.Select(e => e.ErrorMessage)));

        const string code = "ValidationError";
        var status = (int)HttpStatusCode.UnprocessableEntity;
        var errors = exception.Errors
            .Select(e => new { code = "Validation", message = e.ErrorMessage })
            .ToArray();

        var payload = new
        {
            code,
            message = "Один или несколько ошибок валидации.",
            status,
            errors
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred");

        const string code = "InternalServerError";
        var status = (int)HttpStatusCode.InternalServerError;
        var payload = new
        {
            code,
            message = "Внутренняя ошибка сервера.",
            status
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}