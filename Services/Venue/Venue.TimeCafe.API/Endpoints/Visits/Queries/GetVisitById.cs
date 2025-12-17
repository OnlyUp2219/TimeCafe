namespace Venue.TimeCafe.API.Endpoints.Visits.Queries;

public class GetVisitById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/visits/{visitId}", async (
            [FromServices] ISender sender,
            [AsParameters] GetVisitByIdDto visitId) =>
        {
            var query = new GetVisitByIdQuery(visitId.VisitId);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { visit = r.Visit }));
        })
        .WithTags("Visits")
        .WithName("GetVisitById")
        .WithSummary("Получить посещение по ID")
        .WithDescription("Возвращает информацию о посещении по его ID.");
    }
}
