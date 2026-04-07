namespace Venue.TimeCafe.API.Endpoints.Themes.Queries;

public class GetThemeById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/themes/{themeId:guid}", async (
            [FromServices] ISender sender,
            Guid themeId) =>
        {
            var query = new GetThemeByIdQuery(themeId);
            var result = await sender.Send(query);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { theme = r.Theme }));
        })
        .WithTags("Themes")
        .WithName("GetThemeById")
        .WithSummary("Получить тему по ID")
        .WithDescription("Возвращает тему оформления по её идентификатору.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueThemeRead));
    }
}
