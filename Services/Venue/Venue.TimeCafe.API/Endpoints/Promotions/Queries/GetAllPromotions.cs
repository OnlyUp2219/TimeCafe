namespace Venue.TimeCafe.API.Endpoints.Promotions.Queries;

public class GetAllPromotions : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/promotions", async (
            [FromServices] ISender sender) =>
        {
            var query = new GetAllPromotionsQuery();
            var result = await sender.Send(query);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Promotions")
        .WithName("GetAllPromotions")
        .WithSummary("Получить все акции")
        .WithDescription("Возвращает список всех акций в системе.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenuePromotionRead));
    }
}

