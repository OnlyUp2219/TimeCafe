namespace Billing.TimeCafe.Application.DTOs.Balance;

public record BalanceDto(
    Guid UserId,
    decimal CurrentBalance,
    decimal TotalDeposited,
    decimal TotalSpent,
    decimal Debt,
    DateTimeOffset LastUpdated,
    DateTimeOffset CreatedAt);
