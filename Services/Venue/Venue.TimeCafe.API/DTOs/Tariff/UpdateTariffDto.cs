namespace Venue.TimeCafe.API.DTOs.Tariff;

public record UpdateTariffDto(string TariffId, string Name, string? Description, decimal PricePerMinute, int BillingType, string? ThemeId, bool IsActive);

public class UpdateTariffDtoExample : IExamplesProvider<UpdateTariffDto>
{
    public UpdateTariffDto GetExamples() =>
        new(TariffId: "a1111111-1111-1111-1111-111111111111", Name: "Почасовой VIP", Description: "Премиум оплата по часам", PricePerMinute: 10.0m, BillingType: 0, ThemeId: "a1111111-1111-1111-1111-111111111111", IsActive: true);
}
