namespace Venue.TimeCafe.API.DTOs.Tariff;

public record ActivateTariffDto(string TariffId);

public class ActivateTariffExample() : IExamplesProvider<ActivateTariffDto>
{
    public ActivateTariffDto GetExamples() =>
        new(TariffId: "a1111111-1111-1111-1111-111111111111");
}