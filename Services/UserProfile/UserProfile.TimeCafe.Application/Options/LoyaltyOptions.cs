namespace UserProfile.TimeCafe.Application.Options;

public class LoyaltyOptions
{
    // Dictionary mapping VisitCount to DiscountPercent
    public Dictionary<int, decimal> Tiers { get; set; } = new()
    {
        { 5, 5m },
        { 10, 10m },
        { 20, 15m }
    };
}
