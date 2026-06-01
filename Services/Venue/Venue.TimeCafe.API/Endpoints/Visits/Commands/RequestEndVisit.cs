namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class RequestEndVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits/{visitId:guid}/request-end", async (
            [FromServices] ISender sender,
            Guid visitId) =>
        {
            var command = new RequestEndVisitCommand(visitId);
            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Visits")
        .WithName("RequestEndVisit")
        .WithSummary("Запросить завершение")
        .WithDescription("Пользователь запрашивает завершение визита. Таймер продолжает идти.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitEnd));
    }
}

