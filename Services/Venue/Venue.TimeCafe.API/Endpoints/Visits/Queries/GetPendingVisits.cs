namespace Venue.TimeCafe.API.Endpoints.Visits.Queries;

public class GetPendingVisits : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/visits/pending", async (
            [FromServices] ISender sender,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var query = new GetPendingVisitsQuery(page, pageSize);
            var result = await sender.Send(query);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Visits")
        .WithName("GetPendingVisits")
        .WithSummary("Получить ожидающие визиты")
        .WithDescription("Возвращает список визитов в статусе 'Ожидает подтверждения'.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitViewPending));
    }
}
