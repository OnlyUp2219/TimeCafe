namespace BuildingBlocks.Events;

public record VisitTimerStoppedEvent
{
    public Guid VisitId { get; init; }
    public Guid? UserId { get; init; }
    public decimal Amount { get; init; }
    public DateTimeOffset StoppedAt { get; init; }
}
