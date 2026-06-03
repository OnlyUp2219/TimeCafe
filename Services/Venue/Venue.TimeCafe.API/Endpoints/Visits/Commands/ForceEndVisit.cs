namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class ForceEndVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits/{visitId:guid}/force-end", async (
            [FromServices] ISender sender,
            Guid visitId) =>
        {
            var command = new ForceEndVisitCommand(visitId);
            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Visits")
        .WithName("ForceEndVisit")
        .WithSummary("Принудительно завершить")
        .WithDescription("Администратор принудительно завершает визит.")
        .Produces(200)
        .Produces(404)
        .Produces(403)
        .Produces(409)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitForceEnd));
    }
}
