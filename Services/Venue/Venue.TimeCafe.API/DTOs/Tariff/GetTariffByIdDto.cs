namespace Venue.TimeCafe.API.DTOs.Tariff;

public record GetTariffByIdDto(string TariffId);

public class GetTariffByIdExample : IExamplesProvider<GetTariffByIdDto>
{
    public GetTariffByIdDto GetExamples() =>
        new(TariffId: "a1111111-1111-1111-1111-111111111111");
}