namespace Billing.TimeCafe.Application.DTOs.Balance;

public record TransactionDto(
    Guid TransactionId,
    Guid? UserId,
    decimal Amount,
    int Type,
    int Source,
    Guid? SourceId,
    int Status,
    string? Comment,
    DateTimeOffset CreatedAt,
    decimal BalanceAfter);
