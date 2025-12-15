
namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class UpdateVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits/end", async (
            [FromServices] ISender sender,
            [FromBody] UpdateVisitDto dto) =>
        {
            var command = new UpdateVisitCommand(dto.VisitId, dto.UserId, dto.TariffId,dto.EntryTime, dto.ExitTime, dto.CalculatedCost, dto.Status);
            var result = await sender.Send(command);
            result.ToHttpResultV2(onSuccess: r =>
            Results.Ok(new { message = r.Message, visit = r.Visit }));
        });
    }
}