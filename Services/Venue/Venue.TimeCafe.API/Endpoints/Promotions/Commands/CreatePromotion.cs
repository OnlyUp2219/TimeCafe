namespace Venue.TimeCafe.API.Endpoints.Promotions.Commands;

public class CreatePromotion : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/promotions", async (
            ISender sender,
            [FromBody] CreatePromotionDto dto) =>
        {
            var command = new CreatePromotionCommand(dto.Name, dto.Description, dto.DiscountPercent, dto.ValidFrom, dto.ValidTo, dto.IsActive);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Json(new { message = r.Message, promotion = r.Promotion }, statusCode: 201));
        })
        .WithTags("Promotions")
        .WithName("CreatePromotion")
        .WithSummary("Создать акцию")
        .WithDescription("Создаёт новую акцию с указанными параметрами.");
    }
}
