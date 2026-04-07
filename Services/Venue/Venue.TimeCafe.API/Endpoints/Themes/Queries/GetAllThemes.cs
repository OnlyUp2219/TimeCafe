namespace Venue.TimeCafe.API.Endpoints.Themes.Queries;

public class GetAllThemes : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/themes", async (
            [FromServices] ISender sender) =>
        {
            var query = new GetAllThemesQuery();
            var result = await sender.Send(query);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { themes = r.Themes }));
        })
        .WithTags("Themes")
        .WithName("GetAllThemes")
        .WithSummary("Получить все темы")
        .WithDescription("Возвращает список всех доступных тем оформления.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueThemeRead));
    }
}
