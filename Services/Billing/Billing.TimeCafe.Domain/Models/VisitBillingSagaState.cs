namespace Billing.TimeCafe.Domain.Models;

public class VisitBillingSagaState
{
    public Guid VisitId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public Guid? TransactionId { get; set; }
    public VisitBillingSagaStatus Status { get; set; } = VisitBillingSagaStatus.Pending;
    public string? FailureReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? CompensatedAt { get; set; }
}