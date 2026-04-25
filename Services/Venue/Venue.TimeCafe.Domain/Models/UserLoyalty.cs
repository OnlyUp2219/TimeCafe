namespace Venue.TimeCafe.Domain.Models;

public class UserLoyalty
{
    public UserLoyalty()
    {
    }

    public UserLoyalty(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; set; }
    public decimal PersonalDiscountPercent { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}

