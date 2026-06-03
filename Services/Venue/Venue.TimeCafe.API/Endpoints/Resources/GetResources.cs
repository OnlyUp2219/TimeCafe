namespace Venue.TimeCafe.API.Endpoints.Resources;

public class GetResources : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/resources", async ([FromServices] ISender sender) =>
        {
            var result = await sender.Send(new GetResourcesQuery());
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueResourceRead))
        .WithTags("Resources")
        .WithName("GetResources")
        .WithSummary("Получить все столы")
        .WithDescription("Получает список всех физических ресурсов (столов) заведения.")
        .Produces(200);
    }
}
