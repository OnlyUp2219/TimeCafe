namespace Venue.TimeCafe.API.Endpoints.Visits.Queries;

public class GetVisitHistory : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/visits/history/{userId:guid}", async (
            [FromServices] ISender sender,
            Guid userId,
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize) =>
        {
            var query = new GetVisitHistoryQuery(userId, pageNumber, pageSize);
            var result = await sender.Send(query);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { visits = r.Visits }));
        })
        .WithTags("Visits")
        .WithName("GetVisitHistory")
        .WithSummary("Получить историю посещений пользователя")
        .WithDescription("Возвращает историю посещений пользователя с пагинацией.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitRead));
    }
}
