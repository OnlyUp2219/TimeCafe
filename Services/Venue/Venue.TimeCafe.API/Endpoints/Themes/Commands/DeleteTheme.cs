namespace Venue.TimeCafe.API.Endpoints.Themes.Commands;

public class DeleteTheme : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/themes/{themeId:guid}", async (
            [FromServices] ISender sender,
            Guid themeId) =>
        {
            var command = new DeleteThemeCommand(themeId);
            var result = await sender.Send(command);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Themes")
        .WithName("DeleteTheme")
        .WithSummary("Удалить тему")
        .WithDescription("Удаляет тему оформления по идентификатору.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization();
    }
}
