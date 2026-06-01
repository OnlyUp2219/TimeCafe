namespace Venue.TimeCafe.API.Endpoints.ResourceGroups;

public class DeleteResourceGroup : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/resource-groups/{id:guid}", async (Guid id, [FromServices] ISender sender) =>
        {
            var result = await sender.Send(new DeleteResourceGroupCommand(id));
            return result.ToHttpResult(r => TypedResults.NoContent());
        })
        .WithTags("ResourceGroups")
        .WithName("DeleteResourceGroup")
        .WithSummary("Удалить зону")
        .WithDescription("Удаляет указанную зону из заведения.")
        .Produces(204)
        .Produces(404);
    }
}
