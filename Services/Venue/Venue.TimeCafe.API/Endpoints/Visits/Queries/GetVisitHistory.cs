namespace Venue.TimeCafe.API.Endpoints.Visits.Queries;

public class GetVisitHistory : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/visits/history/{userId}", async (
            [FromServices] ISender sender,
            [AsParameters] GetVisitHistoryDto userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) =>
        {
            var query = new GetVisitHistoryQuery(userId.UserId, pageNumber, pageSize);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { visits = r.Visits }));
        })
        .WithTags("Visits")
        .WithName("GetVisitHistory")
        .WithSummary("Получить историю посещений пользователя")
        .WithDescription("Возвращает историю посещений пользователя с пагинацией.");
    }
}
