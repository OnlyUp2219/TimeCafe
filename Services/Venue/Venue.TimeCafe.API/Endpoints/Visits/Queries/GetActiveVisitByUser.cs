namespace Venue.TimeCafe.API.Endpoints.Visits.Queries;

public class GetActiveVisitByUser : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/visits/active/{userId:guid}", async (
            [FromServices] ISender sender,
            Guid userId) =>
        {
            var query = new GetActiveVisitByUserQuery(userId);
            var result = await sender.Send(query);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Visits")
        .WithName("GetActiveVisitByUser")
        .WithSummary("Получить активное посещение пользователя")
        .WithDescription("Возвращает активное посещение для указанного пользователя.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitRead));
    }
}

