namespace Venue.TimeCafe.API.Endpoints.Themes.Queries;

public class GetThemesPage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/themes/page", async (
            [FromServices] ISender sender,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var query = new GetThemesPageQuery(pageNumber <= 0 ? 1 : pageNumber, pageSize <= 0 ? 20 : pageSize);
            var result = await sender.Send(query);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Themes")
        .WithName("GetThemesPage")
        .WithSummary("Получить страницу тем оформления")
        .WithDescription("Возвращает страницу тем оформления с пагинацией.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueThemeRead));
    }
}
