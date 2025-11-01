using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Common.Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Data);
        }

        return ErrorResultHelper.ToHttpResult(result.Errors);
    }
}
