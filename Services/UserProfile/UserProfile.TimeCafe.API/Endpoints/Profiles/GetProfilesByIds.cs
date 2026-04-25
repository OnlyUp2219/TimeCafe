namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public class GetProfilesByIds : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/profiles/batch", async (
            [FromServices] ISender sender,
            [FromBody] List<Guid> userIds) =>
        {
            var query = new GetProfilesByIdsQuery(userIds);
            var result = await sender.Send(query);

            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Profiles")
        .WithName("GetProfilesByIds")
        .WithSummary("Получить список профилей по массиву UserId")
        .WithDescription("Возвращает список профилей для указанных идентификаторов пользователей.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.UserProfileProfileRead));
    }
}

