namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class EndVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits/{visitId:guid}/end", async (
            [FromServices] ISender sender,
            Guid visitId) =>
        {
            var command = new EndVisitCommand(visitId);
            var result = await sender.Send(command);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { message = r.Message, visit = r.Visit, calculatedCost = r.CalculatedCost }));
        })
        .WithTags("Visits")
        .WithName("EndVisit")
        .WithSummary("Завершить посещение")
        .WithDescription("Завершает активное посещение и рассчитывает стоимость.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitEnd));
    }
}
