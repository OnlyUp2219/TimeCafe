namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public class GetProfilesPage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles/page", async (
            [FromServices] ISender sender,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) =>
        {
            var query = new GetProfilesPageQuery(pageNumber, pageSize);
            var result = await sender.Send(query);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new
            {
                message = r.Message,
                profiles = r.Profiles,
                pageNumber = r.PageNumber,
                pageSize = r.PageSize,
                totalCount = r.TotalCount
            }));
        })
        .WithTags("Profiles")
        .WithName("GetProfilesPage")
        .WithSummary("Получить профили с пагинацией")
        .WithDescription("Возвращает страницу профилей с указанными параметрами пагинации.")
        .Produces(200)
        .RequireAuthorization();
    }
}
