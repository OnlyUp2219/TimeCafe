namespace Venue.TimeCafe.API.Endpoints.Tariffs.Commands;

public class UpdateTariff : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/tariffs", async (
            [FromServices] ISender sender,
            [FromBody] UpdateTariffDto dto) =>
        {
            var command = new UpdateTariffCommand
            (
                TariffId: dto.TariffId,
                Name: dto.Name,
                Description: dto.Description!,
                PricePerMinute: dto.PricePerMinute,
                BillingType: (BillingType)dto.BillingType,
                ThemeId: dto.ThemeId,
                IsActive: dto.IsActive
            );

            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message, tariff = r.Tariff }));
        })
        .WithTags("Tariffs")
        .WithName("UpdateTariff")
        .WithSummary("Обновить тариф")
        .WithDescription("Обновляет существующий тариф.")
        .RequireAuthorization();
    }
}
