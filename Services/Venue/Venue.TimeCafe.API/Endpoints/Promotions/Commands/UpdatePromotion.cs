namespace Venue.TimeCafe.API.Endpoints.Promotions.Commands;

public class UpdatePromotion : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/promotions", async (
            [FromServices] ISender sender,
            [FromBody] UpdatePromotionDto dto) =>
        {
            var command = new UpdatePromotionCommand(dto.PromotionId, dto.Name, dto.Description, dto.DiscountPercent, dto.ValidFrom, dto.ValidTo, dto.IsActive);
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
