namespace Venue.TimeCafe.API.Endpoints.Promotions.Queries;

public class GetPromotionById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/promotions/{promotionId}", async (
            ISender sender,
            int promotionId) =>
        {
            var query = new GetPromotionByIdQuery(promotionId);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { promotion = r.Promotion }));
        })
        .WithTags("Promotions")
        .WithName("GetPromotionById")
        .WithSummary("Получить акцию по ID")
        .WithDescription("Возвращает акцию по её идентификатору.");
    }
}
