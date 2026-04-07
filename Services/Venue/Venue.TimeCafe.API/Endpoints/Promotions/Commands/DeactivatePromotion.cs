namespace Venue.TimeCafe.API.Endpoints.Promotions.Commands;

public class DeactivatePromotion : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/promotions/{promotionId:guid}/deactivate", async (
            [FromServices] ISender sender,
            Guid promotionId) =>
        {
            var command = new DeactivatePromotionCommand(promotionId);
            var result = await sender.Send(command);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Promotions")
        .WithName("DeactivatePromotion")
        .WithSummary("Деактивировать акцию")
        .WithDescription("Деактивирует акцию по её идентификатору.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenuePromotionDeactivate));
    }
}
