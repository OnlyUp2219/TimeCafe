namespace BuildingBlocks.Events;

public record VisitRejectedEvent
{
    public Guid VisitId { get; init; }
    public Guid UserId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset RejectedAt { get; init; }
}
