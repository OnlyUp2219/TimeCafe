namespace Venue.TimeCafe.API.DTOs.Tariff;

public record GetTariffsByBillingTypeDto(BillingType BillingType);

public class GetTariffsByBillingTypeExample : IExamplesProvider<GetTariffsByBillingTypeDto>
{
    public GetTariffsByBillingTypeDto GetExamples() =>
        new(BillingType: BillingType.Hourly);
}