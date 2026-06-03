namespace Venue.TimeCafe.API.Endpoints.ResourceGroups;

public class GetResourceGroupById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/resource-groups/{id:guid}", async (Guid id, [FromServices] ISender sender) =>
        {
            var result = await sender.Send(new GetResourceGroupByIdQuery(id));
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueResourceRead))
        .WithTags("ResourceGroups")
        .WithName("GetResourceGroupById")
        .WithSummary("Получить зону по ID")
        .WithDescription("Получает информацию о конкретной пространственной зоне по ее уникальному идентификатору.")
        .Produces(200)
        .Produces(404);
    }
}
