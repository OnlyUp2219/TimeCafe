namespace Venue.TimeCafe.API.Endpoints.Tariffs.Commands;

public record UpdateTariffRequest(string Name, string? Description, decimal PricePerMinute, int BillingType, Guid? ThemeId, bool IsActive);

public class UpdateTariff : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/tariffs/{tariffId:guid}", async (
            [FromServices] ISender sender,
            Guid tariffId,
            [FromBody] UpdateTariffRequest request) =>
        {
            var command = new UpdateTariffCommand
            (
                TariffId: tariffId,
                Name: request.Name,
                Description: request.Description!,
                PricePerMinute: request.PricePerMinute,
                BillingType: (BillingType)request.BillingType,
                ThemeId: request.ThemeId,
                IsActive: request.IsActive
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
