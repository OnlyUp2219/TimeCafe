namespace BuildingBlocks.Events;

public record VisitApprovedEvent
{
    public Guid VisitId { get; init; }
    public Guid? UserId { get; init; }
    public Guid ApprovedByUserId { get; init; }
    public DateTimeOffset ApprovedAt { get; init; }
}
