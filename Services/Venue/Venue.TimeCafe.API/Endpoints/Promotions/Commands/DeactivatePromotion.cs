namespace Venue.TimeCafe.API.Endpoints.Promotions.Commands;

public class DeactivatePromotion : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/promotions/{promotionId}/deactivate", async (
            [FromServices] ISender sender,
            [AsParameters] DeactivatePromotionDto dto) =>
        {
            var command = new DeactivatePromotionCommand(dto.PromotionId);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Promotions")
        .WithName("DeactivatePromotion")
        .WithSummary("Деактивировать акцию")
        .WithDescription("Деактивирует акцию по её идентификатору.");
    }
}
