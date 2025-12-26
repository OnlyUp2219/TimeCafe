namespace Venue.TimeCafe.API.Endpoints.Promotions.Commands;

public class ActivatePromotion : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/promotions/{promotionId}/activate", async (
            [FromServices] ISender sender,
            [AsParameters] ActivatePromotionDto dto) =>
        {
            var command = new ActivatePromotionCommand(dto.PromotionId);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Promotions")
        .WithName("ActivatePromotion")
        .WithSummary("Активировать акцию")
        .WithDescription("Активирует акцию по её идентификатору.")
        .RequireAuthorization();
    }
}
