namespace Billing.TimeCafe.Domain.Errors;

public sealed class BalanceNotFoundError : Error
{
    public BalanceNotFoundError(Guid userId) : base($"Баланс пользователя '{userId}' не найден.")
    {
        Metadata.Add("ErrorCode", "404");
        Metadata.Add("UserId", userId);
    }
}

public sealed class InsufficientFundsError : Error
{
    public InsufficientFundsError(Guid userId, decimal currentBalance) : base($"Недостаточно средств на балансе пользователя '{userId}'. Текущий баланс: {currentBalance}")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("UserId", userId);
        Metadata.Add("CurrentBalance", currentBalance);
    }
}

public sealed class InvalidAmountError : Error
{
    public InvalidAmountError(decimal amount) : base($"Некорректная сумма: {amount}. Сумма должна быть больше нуля.")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("Amount", amount);
    }
}
