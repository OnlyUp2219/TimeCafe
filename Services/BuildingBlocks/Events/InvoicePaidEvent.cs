namespace BuildingBlocks.Events;

public record InvoicePaidEvent
{
    public Guid InvoiceId { get; init; }
    public Guid VisitId { get; init; }
    public Guid? UserId { get; init; }
    public decimal Amount { get; init; }
    public DateTimeOffset PaidAt { get; init; }
}
