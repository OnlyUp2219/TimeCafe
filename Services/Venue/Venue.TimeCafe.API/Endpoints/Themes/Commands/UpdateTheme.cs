namespace Venue.TimeCafe.API.Endpoints.Themes.Commands;

public class UpdateTheme : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/themes", async (
            [FromServices] ISender sender,
            [FromBody] UpdateThemeDto dto) =>
        {
            var command = new UpdateThemeCommand(dto.ThemeId, dto.Name, dto.Emoji, dto.Colors);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message, theme = r.Theme }));
        })
        .WithTags("Themes")
        .WithName("UpdateTheme")
        .WithSummary("Обновить тему")
        .WithDescription("Обновляет существующую тему оформления.")
        .RequireAuthorization();
    }
}
