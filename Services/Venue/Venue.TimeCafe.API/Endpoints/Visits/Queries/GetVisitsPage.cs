namespace Venue.TimeCafe.API.Endpoints.Visits.Queries;

public class GetVisitsPage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/visits/page", async (
            [FromServices] ISender sender,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var query = new GetVisitsPageQuery(page, pageSize);
            var result = await sender.Send(query);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Visits")
        .WithName("GetVisitsPage")
        .WithSummary("Получить страницу визитов")
        .WithDescription("Возвращает страницу визитов с пагинацией для админ-панели.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitRead));
    }
}

