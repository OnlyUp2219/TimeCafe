namespace BuildingBlocks.Events;

public record VisitCompletedEvent
{
    public Guid VisitId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
}
