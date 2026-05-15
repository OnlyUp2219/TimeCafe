namespace Venue.TimeCafe.Domain.Errors;

public sealed class InsufficientFundsError : Error
{
    public InsufficientFundsError() : base("Недостаточно средств на балансе.")
    {
        Metadata.Add("ErrorCode", "400");
    }
}

public sealed class BalanceCheckFailedError : Error
{
    public BalanceCheckFailedError() : base("Не удалось проверить баланс. Повторите попытку позже")
    {
        Metadata.Add("ErrorCode", "503");
    }
}
