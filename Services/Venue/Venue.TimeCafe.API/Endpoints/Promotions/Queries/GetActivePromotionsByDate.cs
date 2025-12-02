namespace Venue.TimeCafe.API.Endpoints.Promotions.Queries;

public class GetActivePromotionsByDate : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/promotions/active/{date}", async (
            ISender sender,
            DateTime date) =>
        {
            var query = new GetActivePromotionsByDateQuery(date);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { promotions = r.Promotions }));
        })
        .WithTags("Promotions")
        .WithName("GetActivePromotionsByDate")
        .WithSummary("Получить активные акции на дату")
        .WithDescription("Возвращает список активных акций на указанную дату.");
    }
}
