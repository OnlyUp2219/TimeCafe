namespace Venue.TimeCafe.API.Endpoints.Promotions.Queries;

public class GetActivePromotionsByDate : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/promotions/active/{date}", async (
            [FromServices] ISender sender,
            DateTimeOffset date) =>
        {
            var query = new GetActivePromotionsByDateQuery(date);
            var result = await sender.Send(query);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Promotions")
        .WithName("GetActivePromotionsByDate")
        .WithSummary("Получить активные акции на дату")
        .WithDescription("Возвращает список активных акций на указанную дату.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenuePromotionRead));
    }
}

