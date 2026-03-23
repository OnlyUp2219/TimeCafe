namespace Venue.TimeCafe.API.Endpoints.Promotions.Commands;

public record CreatePromotionRequest(string Name, string Description, decimal? DiscountPercent, DateTimeOffset ValidFrom, DateTimeOffset ValidTo, bool IsActive);

public class CreatePromotion : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/promotions", async (
            [FromServices] ISender sender,
            [FromBody] CreatePromotionRequest request) =>
        {
            var command = new CreatePromotionCommand(request.Name, request.Description, request.DiscountPercent, request.ValidFrom, request.ValidTo, request.IsActive);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Json(new { message = r.Message, promotion = r.Promotion }, statusCode: 201));
        })
        .WithTags("Promotions")
        .WithName("CreatePromotion")
        .WithSummary("Создать акцию")
        .WithDescription("Создаёт новую акцию с указанными параметрами.")
        .RequireAuthorization();
    }
}
