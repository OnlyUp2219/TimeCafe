namespace Venue.TimeCafe.API.DTOs.Tariff;

public record DeleteTariffDto(string TariffId);

public class DeleteTariffExample : IExamplesProvider<DeleteTariffDto>
{
    public DeleteTariffDto GetExamples() =>
        new(TariffId: "a1111111-1111-1111-1111-111111111111");
}