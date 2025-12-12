namespace Venue.TimeCafe.Domain.DTOs;

public class TariffWithThemeDto
{
    public Guid TariffId { get; set; }
    public string TariffName { get; set; } = string.Empty;
    public string? TariffDescription { get; set; }
    public decimal TariffPricePerMinute { get; set; }
    public BillingType TariffBillingType { get; set; }
    public bool TariffIsActive { get; set; } = true;
    public DateTimeOffset TariffCreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset TariffLastModified { get; set; } = DateTimeOffset.UtcNow;
    public Guid? ThemeId { get; set; }
    public string ThemeName { get; set; } = string.Empty;
    public string? ThemeEmoji { get; set; }
    public string? ThemeColors { get; set; }

}
