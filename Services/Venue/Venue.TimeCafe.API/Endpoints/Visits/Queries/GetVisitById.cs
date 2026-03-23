namespace Venue.TimeCafe.API.Endpoints.Visits.Queries;

public class GetVisitById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/visits/{visitId:guid}", async (
            [FromServices] ISender sender,
            Guid visitId) =>
        {
            var query = new GetVisitByIdQuery(visitId);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { visit = r.Visit }));
        })
        .WithTags("Visits")
        .WithName("GetVisitById")
        .WithSummary("Получить посещение по ID")
        .WithDescription("Возвращает информацию о посещении по его ID.")
        .RequireAuthorization();
    }
}
