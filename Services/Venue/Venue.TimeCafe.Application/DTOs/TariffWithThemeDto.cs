namespace Venue.TimeCafe.Application.DTOs;

public class TariffWithThemeDto
{
    public Guid TariffId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PricePerMinute { get; set; }
    public BillingType BillingType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;

    public Guid? ThemeId { get; set; }
    public string ThemeName { get; set; } = string.Empty;
    public string? ThemeEmoji { get; set; }
    public string? ThemeColors { get; set; }

}
