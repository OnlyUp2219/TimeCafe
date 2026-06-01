namespace Venue.TimeCafe.API.Endpoints.Resources;

public class GetResourceById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/resources/{id:guid}", async (Guid id, [FromServices] ISender sender) =>
        {
            var result = await sender.Send(new GetResourceByIdQuery(id));
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Resources")
        .WithName("GetResourceById")
        .WithSummary("Получить стол по ID")
        .WithDescription("Получает детальную информацию о конкретном физическом ресурсе по его уникальному ID.")
        .Produces(200)
        .Produces(404);
    }
}
