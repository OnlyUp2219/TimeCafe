namespace BuildingBlocks.Extensions;

public static class CqrsResultExtensionsV2
{
    public static IResult ToHttpResultV2<T>(this T result, Func<T, IResult> onSuccess, object? extra = null)
    where T : ICqrsResultV2
    {
        if (result.Success) return onSuccess(result);

        var status = result.StatusCode ?? MapCodeToStatus(result.Code);
        var basePayload = new Dictionary<string, object?>
        {
            ["code"] = result.Code ?? "Error",
            ["message"] = result.Message ?? "Произошла ошибка при обработке запроса",
            ["statusCode"] = status,
            ["errors"] = result.Errors?.Select(e => new { code = e.Code, message = e.Description })
        };

        if (extra != null)
        {
            foreach (var prop in extra.GetType().GetProperties())
            {
                basePayload[prop.Name] = prop.GetValue(extra);
            }
        }
        return Results.Json(basePayload, statusCode: status);
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