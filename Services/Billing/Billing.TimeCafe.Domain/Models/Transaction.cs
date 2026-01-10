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

    public Transaction(string transactionId)
    {
        if (!Guid.TryParse(transactionId, out var guid) || guid == Guid.Empty)
            throw new ArgumentException("Некорректный ID транзакции", nameof(transactionId));
        TransactionId = guid;
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

    public static Transaction CreateDeposit(string userId, decimal amount, TransactionSource source, string? sourceId = null, string? comment = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Сумма должна быть больше нуля", nameof(amount));

        return new Transaction
        {
            UserId = Guid.Parse(userId),
            Amount = amount,
            Type = TransactionType.Deposit,
            Source = source,
            SourceId = string.IsNullOrWhiteSpace(sourceId) ? null : Guid.Parse(sourceId),
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

    public static Transaction CreateWithdrawal(string userId, decimal amount, TransactionSource source, string? sourceId = null, string? comment = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Сумма должна быть больше нуля", nameof(amount));

        return new Transaction
        {
            UserId = Guid.Parse(userId),
            Amount = -amount,
            Type = TransactionType.Withdrawal,
            Source = source,
            SourceId = string.IsNullOrWhiteSpace(sourceId) ? null : Guid.Parse(sourceId),
            Comment = comment,
            Status = TransactionStatus.Completed
        };
    }
}
