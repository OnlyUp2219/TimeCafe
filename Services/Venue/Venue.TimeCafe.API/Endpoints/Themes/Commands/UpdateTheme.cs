namespace Venue.TimeCafe.API.Endpoints.Themes.Commands;

public class UpdateTheme : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/themes", async (
            ISender sender,
            [FromBody] UpdateThemeDto dto) =>
        {
            var theme = new Theme
            {
                ThemeId = dto.ThemeId,
                Name = dto.Name,
                Emoji = dto.Emoji,
                Colors = dto.Colors
            };
            var command = new UpdateThemeCommand(theme);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message, theme = r.Theme }));
        })
        .WithTags("Themes")
        .WithName("UpdateTheme")
        .WithSummary("Обновить тему")
        .WithDescription("Обновляет существующую тему оформления.");
    }
}
