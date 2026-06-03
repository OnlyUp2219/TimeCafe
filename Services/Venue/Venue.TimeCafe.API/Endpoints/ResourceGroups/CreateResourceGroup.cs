namespace Venue.TimeCafe.API.Endpoints.ResourceGroups;

public class CreateResourceGroup : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/resource-groups", async ([FromBody] CreateResourceGroupCommand command, [FromServices] ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Created($"/resource-groups/{r.ResourceGroupId}", r));
        })
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueResourceCreate))
        .WithTags("ResourceGroups")
        .WithName("CreateResourceGroup")
        .WithSummary("Создать новую зону")
        .WithDescription("Создает новую пространственную зону (группу ресурсов) в заведении.")
        .Produces(201)
        .Produces(400);
    }
}
