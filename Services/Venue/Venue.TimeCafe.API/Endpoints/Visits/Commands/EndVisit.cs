namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class EndVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits/end", async (
            [FromServices] ISender sender,
            [FromBody] EndVisitDto dto) =>
        {
            var command = new EndVisitCommand(dto.VisitId);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message, visit = r.Visit, calculatedCost = r.CalculatedCost }));
        })
        .WithTags("Visits")
        .WithName("EndVisit")
        .WithSummary("Завершить посещение")
        .WithDescription("Завершает активное посещение и рассчитывает стоимость.");
    }
}
