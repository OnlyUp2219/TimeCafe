namespace Billing.TimeCafe.Application.DTOs.Balance;

public record TransactionDto(
    Guid TransactionId,
    Guid UserId,
    decimal Amount,
    int Type,
    int Source,
    int Status,
    string? Comment,
    DateTimeOffset CreatedAt,
    decimal BalanceAfter);
