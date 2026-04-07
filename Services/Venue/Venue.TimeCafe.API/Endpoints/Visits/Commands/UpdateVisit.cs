
namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public record UpdateVisitRequest(
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    Guid UserId,
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    Guid TariffId,
    /// <example>2025-01-15T10:30:00+03:00</example>
    DateTimeOffset EntryTime,
    /// <example>2025-01-15T12:30:00+03:00</example>
    DateTimeOffset? ExitTime,
    /// <example>420.00</example>
    decimal? CalculatedCost,
    /// <example>Active</example>
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
            return result.ToHttpResult(onSuccess: r =>
                Results.Ok(new { message = r.Message, visit = r.Visit }));
        })
        .WithTags("Visits")
        .WithName("UpdateVisit")
        .WithSummary("Обновить посещение")
        .WithDescription("Обновляет существующее посещение с новыми данными.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitUpdate));
    }
}
