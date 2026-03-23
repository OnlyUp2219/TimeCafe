namespace Venue.TimeCafe.API.Endpoints.Tariffs.Commands;

public record CreateTariffRequest(string Name, string? Description, decimal PricePerMinute, int BillingType, Guid? ThemeId, bool IsActive);

public class CreateTariff : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/tariffs", async (
            [FromServices] ISender sender,
            [FromBody] CreateTariffRequest request) =>
        {
            var command = new CreateTariffCommand(request.Name, request.Description, request.PricePerMinute, (BillingType)request.BillingType, request.ThemeId, request.IsActive);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Json(new { message = r.Message, tariff = r.Tariff }, statusCode: 201));
        })
        .WithTags("Tariffs")
        .WithName("CreateTariff")
        .WithSummary("Создать тариф")
        .WithDescription("Создаёт новый тариф.")
        .RequireAuthorization();
    }
}
