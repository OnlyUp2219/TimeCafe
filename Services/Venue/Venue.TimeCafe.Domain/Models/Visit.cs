namespace Venue.TimeCafe.Domain.Models;

public class Visit
{
    public int VisitId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int TariffId { get; set; }
    public DateTime EntryTime { get; set; } = DateTime.UtcNow;
    public DateTime? ExitTime { get; set; }
    public decimal? CalculatedCost { get; set; }
    public VisitStatus Status { get; set; } = VisitStatus.Active;

    public virtual Tariff Tariff { get; set; } = null!;
}

public enum VisitStatus
{
    Active = 1,
    Completed = 2
}
