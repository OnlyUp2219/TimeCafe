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
        catch (Exceptions.CqrsResultException ex)
        {
            await HandleCqrsResultExceptionAsync(context, ex);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug("Request was cancelled by the client");
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleKnownExceptionAsync(context, code: "Unauthorized", statusCode: (int)HttpStatusCode.Unauthorized, message: ex.Message);
        }
        catch (System.Security.SecurityException ex)
        {
            await HandleKnownExceptionAsync(context, code: "Forbidden", statusCode: (int)HttpStatusCode.Forbidden, message: ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await HandleKnownExceptionAsync(context, code: "NotFound", statusCode: (int)HttpStatusCode.NotFound, message: ex.Message);
        }
        catch (ArgumentException ex)
        {
            await HandleKnownExceptionAsync(context, code: "BadRequest", statusCode: (int)HttpStatusCode.BadRequest, message: ex.Message);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleCqrsResultExceptionAsync(HttpContext context, Exceptions.CqrsResultException exception)
    {
        var result = exception.Result;
        if (result is null)
        {
            await HandleExceptionAsync(context, exception.InnerException ?? exception);
            return;
        }

        var statusCode = result.StatusCode ?? (int)HttpStatusCode.InternalServerError;
        var errors = result.Errors
            ?.Select(e => new { code = e.Code, message = e.Description })
            .ToArray();

        await WriteErrorAsync(context,
            statusCode,
            payload: new
            {
                code = result.Code ?? "InternalServerError",
                message = string.IsNullOrWhiteSpace(result.Message) ? "Внутренняя ошибка сервера." : result.Message,
                statusCode,
                errors
            });
    }

    private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        _logger.LogWarning("Validation error: {Errors}", string.Join(", ", exception.Errors.Select(e => e.ErrorMessage)));

        const string code = "ValidationError";
        var statusCode = (int)HttpStatusCode.UnprocessableEntity;
        var errors = exception.Errors
            .Select(e => new { code = "Validation", message = e.ErrorMessage })
            .ToArray();

        await WriteErrorAsync(context,
            statusCode,
            payload: new
            {
                code,
                message = "Один или несколько ошибок валидации.",
                statusCode,
                errors
            });
    }

    private async Task HandleKnownExceptionAsync(HttpContext context, string code, int statusCode, string? message)
    {
        await WriteErrorAsync(context,
            statusCode,
            payload: new
            {
                code,
                message = string.IsNullOrWhiteSpace(message) ? "Произошла ошибка при обработке запроса" : message,
                statusCode,
                errors = (object?)null
            });
    }

    private async Task WriteErrorAsync(HttpContext context, int statusCode, object payload)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred");

        const string code = "InternalServerError";
        var statusCode = (int)HttpStatusCode.InternalServerError;

        await WriteErrorAsync(context,
            statusCode,
            payload: new
            {
                code,
                message = "Внутренняя ошибка сервера.",
                statusCode,
                errors = (object?)null
            });
    }
}