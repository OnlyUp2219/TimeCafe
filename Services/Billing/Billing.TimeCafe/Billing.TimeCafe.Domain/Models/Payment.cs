using Billing.TimeCafe.Domain.Enums;

namespace Billing.TimeCafe.Domain.Models;

public class Payment
{
    public Payment()
    {
        PaymentId = Guid.NewGuid();
    }

    public Payment(Guid paymentId)
    {
        PaymentId = paymentId;
    }

    public Guid PaymentId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? ExternalPaymentId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public Guid? TransactionId { get; set; }
    public string? ExternalData { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
