namespace Venue.TimeCafe.API.Endpoints.Resources;

public class UpdateResource : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/resources/{id:guid}", async (Guid id, [FromBody] UpdateResourceCommand command, [FromServices] ISender sender) =>
        {
            if (id != command.ResourceId) return Results.BadRequest("ID стола в маршруте и теле запроса не совпадают");
            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueResourceUpdate))
        .WithTags("Resources")
        .WithName("UpdateResource")
        .WithSummary("Обновить данные стола")
        .WithDescription("Обновляет параметры существующего физического ресурса (стола) по его уникальному ID.")
        .Produces(200)
        .Produces(400);
    }
}
