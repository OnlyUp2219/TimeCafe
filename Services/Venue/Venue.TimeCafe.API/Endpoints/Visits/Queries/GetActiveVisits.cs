namespace Venue.TimeCafe.API.Endpoints.Visits.Queries;

public class GetActiveVisits : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/visits/active", async (
            [FromServices] ISender sender) =>
        {
            var query = new GetActiveVisitsQuery();
            var result = await sender.Send(query);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Visits")
        .WithName("GetActiveVisits")
        .WithSummary("Получить все активные посещения")
        .WithDescription("Возвращает список всех активных посещений.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitRead));
    }
}

