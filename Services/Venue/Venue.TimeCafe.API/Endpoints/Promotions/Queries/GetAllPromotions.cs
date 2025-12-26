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
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { promotions = r.Promotions }));
        })
        .WithTags("Promotions")
        .WithName("GetAllPromotions")
        .WithSummary("Получить все акции")
        .WithDescription("Возвращает список всех акций в системе.")
        .RequireAuthorization();
    }
}
