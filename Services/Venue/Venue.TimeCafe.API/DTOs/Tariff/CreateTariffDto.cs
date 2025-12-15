namespace Venue.TimeCafe.API.DTOs.Tariff;

public record CreateTariffDto(string Name, string? Description, decimal PricePerMinute, int BillingType, string? ThemeId, bool IsActive);

public class CreateTariffDtoExample : IExamplesProvider<CreateTariffDto>
{
    public CreateTariffDto GetExamples() =>
        new(Name: "Почасовой", Description: "Оплата по часам", PricePerMinute: 5.0m, BillingType: 0, ThemeId: "a1111111-1111-1111-1111-111111111111", IsActive: true);
}
