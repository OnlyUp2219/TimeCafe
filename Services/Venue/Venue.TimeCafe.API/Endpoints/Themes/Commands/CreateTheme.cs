namespace Venue.TimeCafe.API.Endpoints.Themes.Commands;

public record CreateThemeRequest(
    /// <example>Космос</example>
    string Name,
    /// <example>🚀</example>
    string? Emoji,
    /// <example>#1a1a2e,#16213e,#0f3460</example>
    string? Colors);

public class CreateTheme : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/themes", async (
            [FromServices] ISender sender,
            [FromBody] CreateThemeRequest request) =>
        {
            var command = new CreateThemeCommand(request.Name, request.Emoji, request.Colors);
            var result = await sender.Send(command);
            return result.ToHttpResult(onSuccess: r => Results.Json(new { message = r.Message, theme = r.Theme }, statusCode: 201));
        })
        .WithTags("Themes")
        .WithName("CreateTheme")
        .WithSummary("Создать тему")
        .WithDescription("Создаёт новую тему оформления для таймкафе.")
        .Produces(201)
        .RequireAuthorization();
    }
}
