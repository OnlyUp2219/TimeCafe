namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public record RejectVisitRequest(string Reason);

public class RejectVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits/{visitId:guid}/reject", async (
            [FromServices] ISender sender,
            Guid visitId,
            [FromBody] RejectVisitRequest request) =>
        {
            var command = new RejectVisitCommand(visitId, request.Reason);
            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Visits")
        .WithName("RejectVisit")
        .WithSummary("Отклонить визит")
        .WithDescription("Отклоняет ожидающий визит с указанием причины.")
        .Produces(200)
        .Produces(404)
        .Produces(409)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitReject));
    }
}
