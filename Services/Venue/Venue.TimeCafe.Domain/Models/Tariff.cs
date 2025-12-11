namespace Venue.TimeCafe.Domain.Models;

public class Tariff
{
    public int TariffId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PricePerMinute { get; set; }
    public BillingType BillingType { get; set; }
    public Guid? ThemeId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    public virtual Theme? Theme { get; set; }
    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
