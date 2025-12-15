namespace Venue.TimeCafe.Domain.Models;

public class Visit
{
    public Visit()
    {
        VisitId = Guid.NewGuid();
    }

    public Visit(Guid visitId)
    {
        VisitId = visitId;
    }

    public Guid VisitId { get; set; }
    public Guid UserId { get; set; }
    public Guid TariffId { get; set; }
    public DateTimeOffset EntryTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExitTime { get; set; }
    public decimal? CalculatedCost { get; set; }
    public VisitStatus Status { get; set; } = VisitStatus.Active;
}

public enum VisitStatus
{
    Active = 1,
    Completed = 2
}
