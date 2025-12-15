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
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { visits = r.Visits }));
        })
        .WithTags("Visits")
        .WithName("GetActiveVisits")
        .WithSummary("Получить все активные посещения")
        .WithDescription("Возвращает список всех активных посещений.");
    }
}
