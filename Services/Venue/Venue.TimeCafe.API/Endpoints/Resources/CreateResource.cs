namespace Venue.TimeCafe.API.Endpoints.Resources;

public class CreateResource : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/resources", async ([FromBody] CreateResourceCommand command, [FromServices] ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Created($"/resources/{r.ResourceId}", r));
        })
        .WithTags("Resources")
        .WithName("CreateResource")
        .WithSummary("Создать новый стол")
        .WithDescription("Создает новый физический ресурс (например, стол, игровую приставку) с привязкой к конкретной зоне.")
        .Produces(201)
        .Produces(400);
    }
}
