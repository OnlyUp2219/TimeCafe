namespace Venue.TimeCafe.API.Endpoints.Themes.Commands;

public record UpdateThemeRequest(string Name, string? Emoji, string? Colors);

public class UpdateTheme : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/themes/{themeId:guid}", async (
            [FromServices] ISender sender,
            Guid themeId,
            [FromBody] UpdateThemeRequest request) =>
        {
            var command = new UpdateThemeCommand(themeId, request.Name, request.Emoji, request.Colors);
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
