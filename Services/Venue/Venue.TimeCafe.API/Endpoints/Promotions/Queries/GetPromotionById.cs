namespace Venue.TimeCafe.API.Endpoints.Promotions.Queries;

public class GetPromotionById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/promotions/{promotionId}", async (
            [FromServices] ISender sender,
            [AsParameters] GetPromotionByIdDto dto) =>
        {
            var query = new GetPromotionByIdQuery(dto.PromotionId);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { promotion = r.Promotion }));
        })
        .WithTags("Promotions")
        .WithName("GetPromotionById")
        .WithSummary("Получить акцию по ID")
        .WithDescription("Возвращает акцию по её идентификатору.");
    }
}
