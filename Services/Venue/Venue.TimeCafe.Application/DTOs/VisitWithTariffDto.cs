namespace Venue.TimeCafe.Application.DTOs;

public class VisitWithTariffDto
{
    public Guid VisitId { get; set; }
    public Guid? UserId { get; set; }
    public Guid TariffId { get; set; }
    public DateTimeOffset EntryTime { get; set; }
    public DateTimeOffset? ExitTime { get; set; }
    public decimal? CalculatedCost { get; set; }
    public VisitStatus Status { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public Guid? ResourceId { get; set; }
    public int? ResourceMaxGuests { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }

    public string TariffName { get; set; } = string.Empty;
    public decimal TariffPricePerMinute { get; set; }
    public string TariffDescription { get; set; } = string.Empty;
    public BillingType TariffBillingType { get; set; }
    public int? TariffMinSessionMinutes { get; set; }
    public string? TariffRoundingRule { get; set; }
    public int? PlannedMinutes { get; set; }
    public int GuestsCount { get; set; }
    public bool IsFinishRequested { get; set; }
    public DateTimeOffset? FinishRequestedAt { get; set; }
}

