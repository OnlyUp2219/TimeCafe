namespace Venue.TimeCafe.API.Endpoints.Themes.Queries;

public class GetThemeById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/themes/{themeId:int}", async (
            ISender sender,
            int themeId) =>
        {
            var query = new GetThemeByIdQuery(themeId);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { theme = r.Theme }));
        })
        .WithTags("Themes")
        .WithName("GetThemeById")
        .WithSummary("Получить тему по ID")
        .WithDescription("Возвращает тему оформления по её идентификатору.");
    }
}
