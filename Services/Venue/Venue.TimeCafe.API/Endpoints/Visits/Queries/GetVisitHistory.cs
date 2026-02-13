namespace Venue.TimeCafe.API.Endpoints.Visits.Queries;

public class GetVisitHistory : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/visits/history/{UserId}", async (
            [FromServices] ISender sender,
            [AsParameters] GetVisitHistoryDto userId) =>
        {
            var query = new GetVisitHistoryQuery(userId.UserId, userId.PageNumber, userId.PageSize);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { visits = r.Visits }));
        })
        .WithTags("Visits")
        .WithName("GetVisitHistory")
        .WithSummary("Получить историю посещений пользователя")
        .WithDescription("Возвращает историю посещений пользователя с пагинацией.")
        .RequireAuthorization();
    }
}
