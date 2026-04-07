namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public record CreateVisitRequest(
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    Guid UserId,
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    Guid TariffId,
    /// <example>120</example>
    int? PlannedMinutes = null,
    /// <example>false</example>
    bool? RequirePositiveBalance = null,
    /// <example>false</example>
    bool? RequireEnoughForPlanned = null);

public class CreateVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits", async (
            [FromServices] ISender sender,
            [FromBody] CreateVisitRequest request) =>
        {
            var command = new CreateVisitCommand(
                request.UserId,
                request.TariffId,
                request.PlannedMinutes,
                request.RequirePositiveBalance ?? true,
                request.RequireEnoughForPlanned ?? false);
            var result = await sender.Send(command);
            return result.ToHttpResult(onSuccess: r => Results.Json(new { message = r.Message, visit = r.Visit }, statusCode: 201));
        })
        .WithTags("Visits")
        .WithName("CreateVisit")
        .WithSummary("Создать посещение")
        .WithDescription("Создаёт новое посещение пользователя по тарифу.")
        .Produces(201)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitCreate));
    }
}
