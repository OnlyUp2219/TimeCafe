namespace TimeCafe.Core.Models;

public record class ClientBalanceDto(
    int ClientId,
    string FullName,
    string PhoneNumber,
    decimal Balance,
    decimal Debt,
    DateTime LastTransactionDate,
    bool IsActive);