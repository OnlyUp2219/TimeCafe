using Microsoft.AspNetCore.Http;
using BuildingBlocks.Common;

namespace BuildingBlocks.Extensions;

public static class ErrorResultHelper
{
    public static IResult ToHttpResult(List<ErrorDetail> errors)
    {
        if (errors.Count == 0)
        {
            return Results.BadRequest(new { type = "Error", message = "Unknown error" });
        }

        var primaryErrorType = DeterminePrimaryErrorType(errors);

        return primaryErrorType switch
        {
            ErrorType.Validation => Results.BadRequest(new { type = "ValidationError", errors }),
            ErrorType.NotFound => Results.NotFound(new { type = "NotFound", errors }),
            ErrorType.Unauthorized => Results.Json(new { type = "Unauthorized", errors }, statusCode: 401),
            ErrorType.Forbidden => Results.Json(new { type = "Forbidden", errors }, statusCode: 403),
            ErrorType.Conflict => Results.Conflict(new { type = "Conflict", errors }),
            ErrorType.RateLimit => Results.Json(new { type = "RateLimit", errors }, statusCode: 429),
            ErrorType.Critical => Results.Json(new { type = "Critical", errors }, statusCode: 500),
            ErrorType.BusinessLogic => Results.BadRequest(new { type = "BusinessLogic", errors }),
            _ => Results.BadRequest(new { type = "Error", errors })
        };
    }
    public static IResult ToHttpResult(ErrorDetail error)
    {
        return ToHttpResult(new List<ErrorDetail> { error });
    }

    private static ErrorType DeterminePrimaryErrorType(List<ErrorDetail> errors)
    {
        if (errors.Count == 0) return ErrorType.Critical;

        if (errors.Any(e => e.Type == ErrorType.Critical)) return ErrorType.Critical;
        if (errors.Any(e => e.Type == ErrorType.Unauthorized)) return ErrorType.Unauthorized;
        if (errors.Any(e => e.Type == ErrorType.Forbidden)) return ErrorType.Forbidden;
        if (errors.Any(e => e.Type == ErrorType.NotFound)) return ErrorType.NotFound;
        if (errors.Any(e => e.Type == ErrorType.RateLimit)) return ErrorType.RateLimit;
        if (errors.Any(e => e.Type == ErrorType.Conflict)) return ErrorType.Conflict;
        if (errors.Any(e => e.Type == ErrorType.Validation)) return ErrorType.Validation;
        if (errors.Any(e => e.Type == ErrorType.BusinessLogic)) return ErrorType.BusinessLogic;

        return errors.First().Type;
    }
}
