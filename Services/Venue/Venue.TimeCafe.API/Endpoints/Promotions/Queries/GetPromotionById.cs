namespace Venue.TimeCafe.API.Endpoints.Promotions.Queries;

public class GetPromotionById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/promotions/{promotionId:guid}", async (
            [FromServices] ISender sender,
            Guid promotionId) =>
        {
            var query = new GetPromotionByIdQuery(promotionId);
            var result = await sender.Send(query);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { promotion = r.Promotion }));
        })
        .WithTags("Promotions")
        .WithName("GetPromotionById")
        .WithSummary("Получить акцию по ID")
        .WithDescription("Возвращает акцию по её идентификатору.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenuePromotionRead));
    }
}
