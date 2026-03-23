namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public record CreateVisitRequest(
    Guid UserId,
    Guid TariffId,
    int? PlannedMinutes = null,
    bool? RequirePositiveBalance = null,
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
            return result.ToHttpResultV2(onSuccess: r => Results.Json(new { message = r.Message, visit = r.Visit }, statusCode: 201));
        })
        .WithTags("Visits")
        .WithName("CreateVisit")
        .WithSummary("Создать посещение")
        .WithDescription("Создаёт новое посещение пользователя по тарифу.")
        .RequireAuthorization();
    }
}
