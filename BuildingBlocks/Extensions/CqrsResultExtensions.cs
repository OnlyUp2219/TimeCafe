namespace BuildingBlocks.Extensions;

public static class CqrsResultExtensions
{
    public static IResult ToHttpResult(this ICqrsResult result, Func<ICqrsResult, IResult> onSuccess)
    {
        if (result.Success) return onSuccess(result);


        var errs = result.Errors?.Select(e =>
        {
            var parts = e.Split(":", 2, StringSplitOptions.RemoveEmptyEntries);
            return new
            {
                code = parts.Length > 0 ? parts[0] : e,
                description = parts.Length > 1 ? parts[1] : e
            };
        }).ToArray();

        return result.TypeError switch
        {
            ETypeError.Unauthorized => Results.Unauthorized(),
            ETypeError.IdentityError => Results.BadRequest(new { errors = errs }),
            ETypeError.BadRequest => Results.BadRequest(new { errors = new { code = result.Errors?.FirstOrDefault(), description = result.Message } }),
            _ => Results.BadRequest(new { message = result.Message })
        };
    }
}
