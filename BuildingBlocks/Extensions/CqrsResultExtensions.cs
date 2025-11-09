namespace BuildingBlocks.Extensions
{
    public static class CqrsResultExtensions
    {
        public static IResult ToHttpResult(this ICqrsResult result, Func<ICqrsResult, IResult> onSuccess)
        {
            if (result.Success) return onSuccess(result);


            var errs = result.Errors?.Select(e => new
            {
                code = e.Split(":", StringSplitOptions.RemoveEmptyEntries)[0],
                description = e.Split(":", StringSplitOptions.RemoveEmptyEntries)[1]
            }).ToArray();

            return result.TypeError switch
            {
                ETypeError.Unauthorized => Results.Unauthorized(),
                ETypeError.IdentityError => Results.BadRequest(new { errors = errs }),
                ETypeError.BadRequest => Results.BadRequest(new { errors = result.Errors }),
                _ => Results.BadRequest(new { message = result.Message })
            };
        }
    }
}
