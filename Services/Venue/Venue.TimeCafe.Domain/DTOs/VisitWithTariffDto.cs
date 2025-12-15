namespace Venue.TimeCafe.Domain.DTOs;

public class VisitWithTariffDto
{
    public Guid VisitId { get; set; }
    public Guid UserId { get; set; }
    public Guid TariffId { get; set; }
    public DateTimeOffset EntryTime { get; set; }
    public DateTimeOffset? ExitTime { get; set; }
    public decimal? CalculatedCost { get; set; }
    public VisitStatus Status { get; set; }

    public string TariffName { get; set; } = string.Empty;
    public decimal TariffPricePerMinute { get; set; }
    public string TariffDescription { get; set; } = string.Empty;
    public BillingType TariffBillingType { get; set; }
}
