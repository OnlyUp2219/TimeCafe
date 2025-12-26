namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class CreateVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits", async (
            [FromServices] ISender sender,
            [FromBody] CreateVisitDto dto) =>
        {
            var command = new CreateVisitCommand(dto.UserId, dto.TariffId);
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
