namespace Venue.TimeCafe.API.DTOs.Tariff;

public record UpdateTariffDto(Guid TariffId, string Name, string? Description, decimal PricePerMinute, int BillingType, Guid? ThemeId, bool IsActive);

public class UpdateTariffDtoExample : IExamplesProvider<UpdateTariffDto>
{
    public UpdateTariffDto GetExamples() =>
        new(TariffId: Guid.Parse("a1111111-1111-1111-1111-111111111111"), Name: "Почасовой VIP", Description: "Премиум оплата по часам", PricePerMinute: 10.0m, BillingType: 0, ThemeId: Guid.Parse("a1111111-1111-1111-1111-111111111111"), IsActive: true);
}
