namespace Venue.TimeCafe.API.Endpoints.Promotions.Commands;

public record CreatePromotionRequest(
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
    /// <example>0</example>
    int Type,
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    Guid? TariffId,
    /// <example>true</example>
    bool IsActive);

public class CreatePromotion : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/promotions", async (
            [FromServices] ISender sender,
            [FromBody] CreatePromotionRequest request) =>
        {
            var command = new CreatePromotionCommand(request.Name, request.Description, request.DiscountPercent, request.ValidFrom, request.ValidTo, (PromotionType)request.Type, request.TariffId, request.IsActive);
            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Created($"/promotions/{r.PromotionId}", r));
        })
        .WithTags("Promotions")
        .WithName("CreatePromotion")
        .WithSummary("Создать акцию")
        .WithDescription("Создаёт новую акцию с указанными параметрами.")
        .Produces(201)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenuePromotionCreate));
    }
}

