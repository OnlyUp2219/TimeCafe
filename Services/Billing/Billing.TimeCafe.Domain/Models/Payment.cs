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

    public Payment(string paymentId)
    {
        if (!Guid.TryParse(paymentId, out var guid) || guid == Guid.Empty)
            throw new ArgumentException("Некорректный ID платежа", nameof(paymentId));
        PaymentId = guid;
    }

    public static Result<Payment> Create(Guid userId, decimal amount, PaymentMethod method = PaymentMethod.Online)
    {
        if (amount <= 0)
            return Result.Fail<Payment>(new InvalidAmountError(amount));

        if (userId == Guid.Empty)
            return Result.Fail<Payment>(new Error("UserId не может быть пустым").WithMetadata("ErrorCode", "400"));

        return Result.Ok(new Payment
        {
            UserId = userId,
            Amount = amount,
            PaymentMethod = method,
            Status = PaymentStatus.Pending
        });
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

    public void MarkAsSucceeded(Guid transactionId, DateTimeOffset? completedAt = null)
    {
        if (Status == PaymentStatus.Completed) return;
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Нельзя перевести платёж из статуса {Status} в статус Completed");
        
        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        CompletedAt = completedAt ?? DateTimeOffset.UtcNow;
        ErrorMessage = null;
    }

    public void MarkAsFailed(string message)
    {
        if (Status == PaymentStatus.Failed) return;
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Нельзя перевести платёж из статуса {Status} в статус Failed");
        
        Status = PaymentStatus.Failed;
        ErrorMessage = message;
    }

    public void MarkAsCancelled(string? message = null)
    {
        if (Status == PaymentStatus.Cancelled) return;
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Нельзя перевести платёж из статуса {Status} в статус Cancelled");
        
        Status = PaymentStatus.Cancelled;
        ErrorMessage = message ?? "Платёж отменён";
    }

    public void UpdateExternalData(string data)
    {
        ExternalData = data;
    }
}
