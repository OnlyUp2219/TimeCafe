using BuildingBlocks.Extensions;
using Microsoft.AspNetCore.Http;

namespace Auth.TimeCafe.Application.Extensions;

/// <summary>
/// Extension методы для Result
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Преобразует Result в соответствующий HTTP ответ
    /// </summary>
    public static IResult ToHttpResult<T>(this Common.Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Data);
        }

        return ErrorResultHelper.ToHttpResult(result.Errors);
    }
}
