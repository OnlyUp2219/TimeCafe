using Billing.TimeCafe.Domain.Enums;

namespace Billing.TimeCafe.Domain.Models;

public class Transaction
{
    public Transaction()
    {
        TransactionId = Guid.NewGuid();
    }

    public Transaction(Guid transactionId)
    {
        TransactionId = transactionId;
    }

    public Guid TransactionId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public TransactionSource Source { get; set; }
    public Guid? SourceId { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public decimal BalanceAfter { get; set; }

    public static Transaction CreateDeposit(Guid userId, decimal amount, TransactionSource source, Guid? sourceId = null, string? comment = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Сумма должна быть больше нуля", nameof(amount));

        return new Transaction
        {
            UserId = userId,
            Amount = amount,
            Type = TransactionType.Deposit,
            Source = source,
            SourceId = sourceId,
            Comment = comment,
            Status = TransactionStatus.Completed
        };
    }

    public static Transaction CreateWithdrawal(Guid userId, decimal amount, TransactionSource source, Guid? sourceId = null, string? comment = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Сумма должна быть больше нуля", nameof(amount));

        return new Transaction
        {
            UserId = userId,
            Amount = -amount, 
            Type = TransactionType.Withdrawal,
            Source = source,
            SourceId = sourceId,
            Comment = comment,
            Status = TransactionStatus.Completed
        };
    }
}
