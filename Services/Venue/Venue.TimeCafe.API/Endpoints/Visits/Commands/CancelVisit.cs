namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class CancelVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits/{visitId:guid}/cancel", async (
            [FromServices] ISender sender,
            Guid visitId,
            ClaimsPrincipal principal) =>
        {
            var userId = Guid.Parse(principal.FindFirstValue("sub")!);
            var command = new CancelVisitCommand(visitId, userId);
            var result = await sender.Send(command);
            return result.ToHttpResult(() => TypedResults.Ok());
        })
        .WithTags("Visits")
        .WithName("CancelVisit")
        .WithSummary("Отменить визит")
        .WithDescription("Отменяет ожидающий визит пользователем.")
        .Produces(200)
        .Produces(404)
        .Produces(403)
        .Produces(409)
        .RequireAuthorization();
    }
}
