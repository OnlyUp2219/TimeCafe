namespace Venue.TimeCafe.API.Endpoints.Promotions.Queries;

public class GetActivePromotions : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/promotions/active", async (
            ISender sender) =>
        {
            var query = new GetActivePromotionsQuery();
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { promotions = r.Promotions }));
        })
        .WithTags("Promotions")
        .WithName("GetActivePromotions")
        .WithSummary("Получить активные акции")
        .WithDescription("Возвращает список всех активных акций.");
    }
}
