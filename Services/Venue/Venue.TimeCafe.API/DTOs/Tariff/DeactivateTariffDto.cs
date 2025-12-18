namespace Venue.TimeCafe.API.DTOs.Tariff;

public record DeactivateTariffDto(string TariffId);

public class DeactivateTariffExample : IExamplesProvider<DeactivateTariffDto>
{
    public DeactivateTariffDto GetExamples() =>
        new(TariffId: "a1111111-1111-1111-1111-111111111111");
}