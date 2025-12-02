namespace Venue.TimeCafe.API.DTOs.Tariff;

public record UpdateTariffDto(int TariffId, string Name, string? Description, decimal PricePerMinute, int BillingType, int? ThemeId, bool IsActive);

public class UpdateTariffDtoExample : IExamplesProvider<UpdateTariffDto>
{
    public UpdateTariffDto GetExamples() =>
        new(TariffId: 1, Name: "Почасовой VIP", Description: "Премиум оплата по часам", PricePerMinute: 10.0m, BillingType: 0, ThemeId: 1, IsActive: true);
}
