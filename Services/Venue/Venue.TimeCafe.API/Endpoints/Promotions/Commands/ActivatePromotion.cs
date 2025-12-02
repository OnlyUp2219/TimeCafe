namespace Venue.TimeCafe.API.Endpoints.Promotions.Commands;

public class ActivatePromotion : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/promotions/{promotionId}/activate", async (
            ISender sender,
            int promotionId) =>
        {
            var command = new ActivatePromotionCommand(promotionId);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Promotions")
        .WithName("ActivatePromotion")
        .WithSummary("Активировать акцию")
        .WithDescription("Активирует акцию по её идентификатору.");
    }
}
