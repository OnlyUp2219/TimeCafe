namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public class GetProfileById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles/{userId:guid}", async (
            [FromServices] ISender sender,
            Guid userId) =>
        {
            var query = new GetProfileByIdQuery(userId);
            var result = await sender.Send(query);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Profiles")
        .WithName("GetProfileById")
        .WithSummary("Получить профиль по UserId")
        .WithDescription("Возвращает профиль пользователя по идентификатору UserId.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.UserProfileProfileRead));
    }
}

