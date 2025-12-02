namespace Venue.TimeCafe.API.Endpoints.Promotions.Commands;

public class UpdatePromotion : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/promotions", async (
            ISender sender,
            [FromBody] UpdatePromotionDto dto) =>
        {
            var promotion = new Promotion
            {
                PromotionId = dto.PromotionId,
                Name = dto.Name,
                Description = dto.Description,
                DiscountPercent = dto.DiscountPercent,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                IsActive = dto.IsActive
            };
            var command = new UpdatePromotionCommand(promotion);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message, promotion = r.Promotion }));
        })
        .WithTags("Promotions")
        .WithName("UpdatePromotion")
        .WithSummary("Обновить акцию")
        .WithDescription("Обновляет существующую акцию с новыми данными.");
    }
}
