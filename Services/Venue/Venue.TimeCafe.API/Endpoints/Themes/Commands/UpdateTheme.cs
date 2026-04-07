namespace Venue.TimeCafe.API.Endpoints.Themes.Commands;

public record UpdateThemeRequest(
    /// <example>Космос</example>
    string Name,
    /// <example>🚀</example>
    string? Emoji,
    /// <example>#1a1a2e,#16213e,#0f3460</example>
    string? Colors);

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
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { message = r.Message, theme = r.Theme }));
        })
        .WithTags("Themes")
        .WithName("UpdateTheme")
        .WithSummary("Обновить тему")
        .WithDescription("Обновляет существующую тему оформления.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueThemeUpdate));
    }
}
