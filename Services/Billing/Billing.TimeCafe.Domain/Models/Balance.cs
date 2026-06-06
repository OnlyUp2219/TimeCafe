namespace Billing.TimeCafe.Domain.Models;

public class Balance
{
    public Balance()
    {
        UserId = Guid.Empty;
    }

    public Balance(Guid userId)
    {
        UserId = userId;
    }

    public Balance(string userId)
    {
        if (!Guid.TryParse(userId, out var guid) || guid == Guid.Empty)
            throw new ArgumentException("Некорректный ID пользователя", nameof(userId));
        UserId = guid;
    }

    public static Balance Create(Guid userId) => new(userId);

    public Guid UserId { get; set; }
    public decimal CurrentBalance { get; set; } = 0;
    public decimal TotalDeposited { get; set; } = 0;
    public decimal TotalSpent { get; set; } = 0;
    public decimal Debt { get; set; } = 0;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Result Deposit(decimal amount)
    {
        if (amount <= 0)
            return Result.Fail(new InvalidAmountError(amount));

        CurrentBalance += amount;
        TotalDeposited += amount;
        LastUpdated = DateTimeOffset.UtcNow;

        return Result.Ok();
    }

    public Result Withdraw(decimal amount, bool allowDebt = false)
    {
        if (amount <= 0)
            return Result.Fail(new InvalidAmountError(amount));

        if (!allowDebt && CurrentBalance < amount)
            return Result.Fail(new InsufficientFundsError(UserId, CurrentBalance));

        CurrentBalance -= amount;
        TotalSpent += amount;

        if (CurrentBalance < 0)
        {
            Debt = Math.Abs(CurrentBalance);
            CurrentBalance = 0;
        }

        LastUpdated = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    public void ForgiveDebt()
    {
        Debt = 0;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void RecordSpent(decimal amount)
    {
        if (amount <= 0)
            return;

        TotalSpent += amount;
        LastUpdated = DateTimeOffset.UtcNow;
    }
}
