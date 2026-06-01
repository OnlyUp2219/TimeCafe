namespace Venue.TimeCafe.API.Endpoints.Resources;

public class DeleteResource : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/resources/{id:guid}", async (Guid id, [FromServices] ISender sender) =>
        {
            var result = await sender.Send(new DeleteResourceCommand(id));
            return result.ToHttpResult(r => TypedResults.NoContent());
        })
        .WithTags("Resources")
        .WithName("DeleteResource")
        .WithSummary("Удалить стол")
        .WithDescription("Удаляет конкретный физический ресурс по его уникальному ID.")
        .Produces(204)
        .Produces(404);
    }
}
