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
    
    public string? Summary { get; set; }
    public List<string>? Features { get; set; }
    public List<string>? AudienceTags { get; set; }
    public int? MinSessionMinutes { get; set; }
    public string? RoundingRule { get; set; }
    public int? MaxGuests { get; set; }
    public string? CancellationPolicy { get; set; }
    public bool IsRecommended { get; set; }
    public int SortOrder { get; set; }
}

