namespace Venue.TimeCafe.API.Endpoints.Promotions.Commands;

public record UpdatePromotionRequest(string Name, string Description, decimal? DiscountPercent, DateTimeOffset ValidFrom, DateTimeOffset ValidTo, bool IsActive);

public class UpdatePromotion : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/promotions/{promotionId:guid}", async (
            [FromServices] ISender sender,
            Guid promotionId,
            [FromBody] UpdatePromotionRequest request) =>
        {
            var command = new UpdatePromotionCommand(promotionId, request.Name, request.Description, request.DiscountPercent, request.ValidFrom, request.ValidTo, request.IsActive);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message, promotion = r.Promotion }));
        })
        .WithTags("Promotions")
        .WithName("UpdatePromotion")
        .WithSummary("Обновить акцию")
        .WithDescription("Обновляет существующую акцию с новыми данными.")
        .RequireAuthorization();
    }
}
