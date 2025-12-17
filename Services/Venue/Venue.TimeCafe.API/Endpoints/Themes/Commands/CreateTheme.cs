namespace Venue.TimeCafe.API.Endpoints.Themes.Commands;

public class CreateTheme : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/themes", async (
            [FromServices] ISender sender,
            [FromBody] CreateThemeDto dto) =>
        {
            var command = new CreateThemeCommand(dto.Name, dto.Emoji, dto.Colors);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Json(new { message = r.Message, theme = r.Theme }, statusCode: 201));
        })
        .WithTags("Themes")
        .WithName("CreateTheme")
        .WithSummary("Создать тему")
        .WithDescription("Создаёт новую тему оформления для таймкафе.");
    }
}
