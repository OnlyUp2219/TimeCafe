
namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public record UpdateVisitRequest(
    Guid UserId,
    Guid TariffId,
    DateTimeOffset EntryTime,
    DateTimeOffset? ExitTime,
    decimal? CalculatedCost,
    VisitStatus Status);

public class UpdateVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/visits/{visitId:guid}", async (
            [FromServices] ISender sender,
            Guid visitId,
            [FromBody] UpdateVisitRequest request) =>
        {
            var command = new UpdateVisitCommand(visitId, request.UserId, request.TariffId, request.EntryTime, request.ExitTime, request.CalculatedCost, request.Status);
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
