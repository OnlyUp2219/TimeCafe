namespace Venue.TimeCafe.API.Endpoints.Promotions.Commands;

public class DeletePromotion : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/promotions/{promotionId:guid}", async (
            [FromServices] ISender sender,
            Guid promotionId) =>
        {
            var command = new DeletePromotionCommand(promotionId);
            var result = await sender.Send(command);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Promotions")
        .WithName("DeletePromotion")
        .WithSummary("Удалить акцию")
        .WithDescription("Удаляет акцию по её идентификатору.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenuePromotionDelete));
    }
}
