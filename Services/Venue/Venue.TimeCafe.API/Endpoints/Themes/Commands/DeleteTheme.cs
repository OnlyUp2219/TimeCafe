namespace Venue.TimeCafe.API.Endpoints.Themes.Commands;

public class DeleteTheme : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/themes/{themeId}", async (
            [FromServices] ISender sender,
            [AsParameters] DeleteThemeDto dto) =>
        {
            var command = new DeleteThemeCommand(dto.ThemeId);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Themes")
        .WithName("DeleteTheme")
        .WithSummary("Удалить тему")
        .WithDescription("Удаляет тему оформления по идентификатору.")
        .RequireAuthorization();
    }
}
