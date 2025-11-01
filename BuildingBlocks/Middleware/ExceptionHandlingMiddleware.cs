namespace BuildingBlocks.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleUnauthorizedExceptionAsync(context, ex);
        }
        catch (KeyNotFoundException ex)
        {
            await HandleNotFoundExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        _logger.LogWarning("Validation error: {Errors}", string.Join(", ", exception.Errors.Select(e => e.ErrorMessage)));

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var errors = exception.Errors.Select(e => new ErrorDetail(
            ErrorType.Validation,
            $"Validation.{e.PropertyName}",
            e.ErrorMessage,
            e.PropertyName
        )).ToList();

        var response = new
        {
            type = "ValidationError",
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private async Task HandleUnauthorizedExceptionAsync(HttpContext context, UnauthorizedAccessException exception)
    {
        _logger.LogWarning("Unauthorized access: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

        var error = ErrorDetail.Unauthorized(exception.Message);

        var response = new
        {
            type = "Unauthorized",
            error
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private async Task HandleNotFoundExceptionAsync(HttpContext context, KeyNotFoundException exception)
    {
        _logger.LogWarning("Resource not found: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

        var error = ErrorDetail.NotFound("Resource");

        var response = new
        {
            type = "NotFound",
            error
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var error = ErrorDetail.Critical("Произошла непредвиденная ошибка. Пожалуйста, попробуйте позже.");

        var response = new
        {
            type = "Critical",
            error
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
