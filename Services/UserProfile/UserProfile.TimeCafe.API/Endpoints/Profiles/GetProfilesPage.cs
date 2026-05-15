namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public class GetProfilesPage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles/page", async (
            [FromServices] ISender sender,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var query = new GetProfilesPageQuery(page, pageSize);
            var result = await sender.Send(query);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Profiles")
        .WithName("GetProfilesPage")
        .WithSummary("Получить профили с пагинацией")
        .WithDescription("Возвращает страницу профилей с указанными параметрами пагинации.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.UserProfileProfileRead));
    }
}

