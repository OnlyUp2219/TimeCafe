namespace Venue.TimeCafe.Domain.DTOs;

public class VisitWithTariffDto
{
    public Guid VisitId { get; set; }
    public Guid UserId { get; set; }
    public Guid TariffId { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public decimal? CalculatedCost { get; set; }
    public int Status { get; set; }

    public string TariffName { get; set; } = string.Empty;
    public decimal TariffPricePerMinute { get; set; }
    public string TariffDescription { get; set; } = string.Empty;
}
