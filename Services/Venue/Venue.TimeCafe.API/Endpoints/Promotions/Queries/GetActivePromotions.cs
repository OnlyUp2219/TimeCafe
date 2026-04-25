namespace Venue.TimeCafe.API.Endpoints.Promotions.Queries;

public class GetActivePromotions : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/promotions/active", async (
            [FromServices] ISender sender) =>
        {
            var query = new GetActivePromotionsQuery();
            var result = await sender.Send(query);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Promotions")
        .WithName("GetActivePromotions")
        .WithSummary("Получить активные акции")
        .WithDescription("Возвращает список всех активных акций.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenuePromotionRead));
    }
}

