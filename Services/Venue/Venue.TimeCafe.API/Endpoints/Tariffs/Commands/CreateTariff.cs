namespace Venue.TimeCafe.API.Endpoints.Tariffs.Commands;

public class CreateTariff : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/tariffs", async (
            [FromServices] ISender sender,
            [FromBody] CreateTariffDto dto) =>
        {
            var command = new CreateTariffCommand(dto.Name, dto.Description, dto.PricePerMinute, (BillingType)dto.BillingType, dto.ThemeId, dto.IsActive);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Json(new { message = r.Message, tariff = r.Tariff }, statusCode: 201));
        })
        .WithTags("Tariffs")
        .WithName("CreateTariff")
        .WithSummary("Создать тариф")
        .WithDescription("Создаёт новый тариф.");
    }
}
