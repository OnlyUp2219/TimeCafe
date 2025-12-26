namespace Venue.TimeCafe.API.Endpoints.Visits.Queries;

public class GetActiveVisitByUser : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/visits/active/{userId}", async (
            [FromServices] ISender sender,
            [AsParameters] GetActiveVisitByUserDto userId) =>
        {
            var query = new GetActiveVisitByUserQuery(userId.UserId);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { visit = r.Visit }));
        })
        .WithTags("Visits")
        .WithName("GetActiveVisitByUser")
        .WithSummary("Получить активное посещение пользователя")
        .WithDescription("Возвращает активное посещение для указанного пользователя.")
        .RequireAuthorization();
    }
}
