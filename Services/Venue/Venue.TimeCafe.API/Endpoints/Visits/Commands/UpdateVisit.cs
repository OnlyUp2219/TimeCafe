
namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class UpdateVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/visits", async (
            [FromServices] ISender sender,
            [FromBody] UpdateVisitDto dto) =>
        {
            var command = new UpdateVisitCommand(dto.VisitId, dto.UserId, dto.TariffId, dto.EntryTime, dto.ExitTime, dto.CalculatedCost, dto.Status);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r =>
                Results.Ok(new { message = r.Message, visit = r.Visit }));
        })
        .WithTags("Visits")
        .WithName("UpdateVisit")
        .WithSummary("Обновить посещение")
        .WithDescription("Обновляет существующее посещение с новыми данными.")
        .RequireAuthorization();
    }
}