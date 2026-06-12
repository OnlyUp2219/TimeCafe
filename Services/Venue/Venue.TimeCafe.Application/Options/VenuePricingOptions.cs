namespace Venue.TimeCafe.Application.Options;

public class VenuePricingOptions
{
    public decimal MaxTotalDiscountPercent { get; set; } = 50m;
    public int GracePeriodMinutes { get; set; } = 3;
}

