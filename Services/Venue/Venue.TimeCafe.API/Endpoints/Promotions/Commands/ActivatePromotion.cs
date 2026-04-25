namespace Venue.TimeCafe.API.Endpoints.Promotions.Commands;

public class ActivatePromotion : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/promotions/{promotionId:guid}/activate", async (
            [FromServices] ISender sender,
            Guid promotionId) =>
        {
            var command = new ActivatePromotionCommand(promotionId);
            var result = await sender.Send(command);
            return result.ToHttpResult(() => TypedResults.NoContent());
        })
        .WithTags("Promotions")
        .WithName("ActivatePromotion")
        .WithSummary("Активировать акцию")
        .WithDescription("Активирует акцию по её идентификатору.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenuePromotionActivate));
    }
}

