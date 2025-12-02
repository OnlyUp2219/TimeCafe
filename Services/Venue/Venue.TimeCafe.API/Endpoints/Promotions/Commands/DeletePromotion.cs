namespace Venue.TimeCafe.API.Endpoints.Promotions.Commands;

public class DeletePromotion : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/promotions/{promotionId}", async (
            ISender sender,
            int promotionId) =>
        {
            var command = new DeletePromotionCommand(promotionId);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Promotions")
        .WithName("DeletePromotion")
        .WithSummary("Удалить акцию")
        .WithDescription("Удаляет акцию по её идентификатору.");
    }
}
