using FluentResults;

namespace BuildingBlocks.Extensions;

public static class CqrsResultExtensions
{
    public static IResult ToHttpResult<T>(this T result, Func<T, IResult> onSuccess, object? extra = null)
    where T : ICqrsResult
    {
        if (result.Success)
            return onSuccess(result);

        var statusCode = result.StatusCode ?? MapCodeToStatus(result.Code);
        var message = result.Message ?? "Произошла ошибка при обработке запроса";
        var errors = result.Errors?
            .Where(error => !string.IsNullOrWhiteSpace(error.Description))
            .Select(error => (object)new Dictionary<string, object?>
            {
                ["code"] = error.Code,
                ["message"] = error.Description
            })
            .ToList();

        return BuildErrorResponse(message, statusCode, result.Code, errors, extra);
    }

    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess, object? extra = null)
    {
        if (result.IsSuccess)
            return onSuccess(result.Value);

        return BuildFluentErrorResponse(result.Errors, extra);
    }

    public static IResult ToHttpResult(this Result result, Func<IResult> onSuccess, object? extra = null)
    {
        if (result.IsSuccess)
            return onSuccess();

        return BuildFluentErrorResponse(result.Errors, extra);
    }

    private static IResult BuildFluentErrorResponse(IReadOnlyList<IError> errors, object? extra)
    {
        var firstError = errors.FirstOrDefault();
        var statusCode = ExtractStatusCode(firstError) ?? StatusCodes.Status400BadRequest;
        var message = firstError?.Message ?? "Произошла ошибка при обработке запроса";
        var code = ExtractErrorCode(firstError);
        var payloadErrors = errors
            .Where(error => !string.IsNullOrWhiteSpace(error.Message))
            .Select(error => (object)new Dictionary<string, object?>
            {
                ["code"] = ExtractErrorCode(error) ?? code ?? "Error",
                ["message"] = error.Message
            })
            .ToList();

        return BuildErrorResponse(message, statusCode, code, payloadErrors, extra);
    }

    private static IResult BuildErrorResponse(string message, int statusCode, string? code, List<object>? errors, object? extra)
    {
        var payload = new Dictionary<string, object?>
        {
            ["message"] = message,
            ["statusCode"] = statusCode
        };

        if (!string.IsNullOrWhiteSpace(code))
            payload["code"] = code;

        if (errors is { Count: > 0 })
            payload["errors"] = errors;

        if (extra != null)
        {
            foreach (var prop in extra.GetType().GetProperties())
            {
                payload[prop.Name] = prop.GetValue(extra);
            }
        }

        return Results.Json(payload, statusCode: statusCode);
    }

    private static string? ExtractErrorCode(IError? error)
    {
        if (error is null)
            return null;

        if (error.Metadata.TryGetValue("Code", out var rawCode))
            return rawCode?.ToString();

        return null;
    }

    private static int? ExtractStatusCode(IError? error)
    {
        if (error is null)
            return null;

        if (TryGetStatusCode(error.Metadata, "ErrorCode", out var statusCode))
            return statusCode;

        if (TryGetStatusCode(error.Metadata, "StatusCode", out statusCode))
            return statusCode;

        return null;
    }

    private static bool TryGetStatusCode(IReadOnlyDictionary<string, object> metadata, string key, out int statusCode)
    {
        statusCode = default;

        if (!metadata.TryGetValue(key, out var rawStatusCode))
            return false;

        return int.TryParse(rawStatusCode?.ToString(), out statusCode);
    }

    private static int MapCodeToStatus(string? code) => code switch
    {
        "BadRequest" => 400,
        "Unauthorized" => 401,
        "Forbidden" => 403,
        "NotFound" => 404,
        "Conflict" => 409,
        "ValidationError" => 422,
        "IdentityError" => 400,
        "RateLimit" => 429,
        "ExternalServiceError" => 502,
        "InternalServerError" => 500,
        _ => 400
    };
}
