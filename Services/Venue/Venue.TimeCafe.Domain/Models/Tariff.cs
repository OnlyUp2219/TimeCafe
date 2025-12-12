namespace Venue.TimeCafe.Domain.Models;

public class Tariff
{
    public Tariff(Guid themeId)
    {
        TariffId = themeId;
    }

    public Tariff()
    {
        TariffId = Guid.NewGuid();
    }

    public Guid TariffId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PricePerMinute { get; set; }
    public BillingType BillingType { get; set; }
    public Guid? ThemeId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
