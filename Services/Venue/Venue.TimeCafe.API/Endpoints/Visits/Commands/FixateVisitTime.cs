namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class FixateVisitTime : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits/{visitId:guid}/fixate-time", async (
            [FromServices] ISender sender,
            Guid visitId) =>
        {
            var command = new FixateVisitTimeCommand(visitId);
            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Visits")
        .WithName("FixateVisitTime")
        .WithSummary("Зафиксировать время визита")
        .WithDescription("Администратор фиксирует время визита. Переводит статус в WaitingForPayment.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitUpdate));
    }
}
