namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class WalkInVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits/walk-in", async (
            [FromServices] ISender sender,
            [FromBody] WalkInVisitCommand command) =>
        {
            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Visits")
        .WithName("WalkInVisit")
        .WithSummary("Посадить гостя (Walk-in)")
        .WithDescription("Создает активный визит для гостя (с регистрацией или без).")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitCreate));
    }
}
