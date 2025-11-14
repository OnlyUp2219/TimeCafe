namespace BuildingBlocks.Extensions;

public static class CqrsResultExtensionsV2
{
    public static IResult ToHttpResultV2<T>(this T result, Func<T, IResult> onSuccess)
    where T : ICqrsResultV2
    {
        if (result.Success) return onSuccess(result);

        var status = result.StatusCode ?? MapCodeToStatus(result.Code);
        var payload = new
        {
            code = result.Code ?? "Error",
            message = result.Message ?? "Произошла ошибка при обработке запроса",
            statusCode = status,
            errors = result.Errors?.Select(e => new { code = e.Code, message = e.Description })
        };
        return Results.Json(payload, statusCode: status);
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