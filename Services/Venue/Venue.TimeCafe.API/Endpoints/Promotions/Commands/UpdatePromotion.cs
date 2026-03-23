namespace Venue.TimeCafe.API.Endpoints.Promotions.Commands;

public record UpdatePromotionRequest(
    /// <example>Летняя акция</example>
    string Name,
    /// <example>Скидка 20% на все тарифы в летний период</example>
    string Description,
    /// <example>20.0</example>
    decimal? DiscountPercent,
    /// <example>2025-06-01T00:00:00+03:00</example>
    DateTimeOffset ValidFrom,
    /// <example>2025-08-31T23:59:59+03:00</example>
    DateTimeOffset ValidTo,
    /// <example>true</example>
    bool IsActive);

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
        .Produces(200)
        .Produces(404)
        .RequireAuthorization();
    }
}
