namespace Venue.TimeCafe.API.Endpoints.Themes.Commands;

public class DeleteTheme : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/themes/{themeId:int}", async (
            [FromServices] ISender sender,
            [FromRoute] string themeId) =>
        {
            // TODO : DTO
            var command = new DeleteThemeCommand(themeId);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Themes")
        .WithName("DeleteTheme")
        .WithSummary("Удалить тему")
        .WithDescription("Удаляет тему оформления по идентификатору.");
    }
}
