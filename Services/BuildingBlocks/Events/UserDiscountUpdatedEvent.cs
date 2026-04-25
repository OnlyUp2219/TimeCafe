namespace BuildingBlocks.Events;

public record UserDiscountUpdatedEvent
{
    public Guid UserId { get; init; }
    public decimal PersonalDiscountPercent { get; init; }
}
