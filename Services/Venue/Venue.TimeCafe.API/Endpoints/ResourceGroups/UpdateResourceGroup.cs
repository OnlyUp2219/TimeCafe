namespace Venue.TimeCafe.API.Endpoints.ResourceGroups;

public class UpdateResourceGroup : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/resource-groups/{id:guid}", async (Guid id, [FromBody] UpdateResourceGroupCommand command, [FromServices] ISender sender) =>
        {
            if (id != command.ResourceGroupId) return Results.BadRequest("ID зоны в маршруте и теле запроса не совпадают");
            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueResourceUpdate))
        .WithTags("ResourceGroups")
        .WithName("UpdateResourceGroup")
        .WithSummary("Обновить данные зоны")
        .WithDescription("Обновляет параметры существующей пространственной зоны по ее ID.")
        .Produces(200)
        .Produces(400);
    }
}
