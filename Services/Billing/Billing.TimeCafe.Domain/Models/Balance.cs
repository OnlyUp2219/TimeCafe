using Billing.TimeCafe.Domain.Enums;

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

    public Guid UserId { get; set; }
    public decimal CurrentBalance { get; set; } = 0;
    public decimal TotalDeposited { get; set; } = 0;
    public decimal TotalSpent { get; set; } = 0;
    public decimal Debt { get; set; } = 0;
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Сумма пополнения должна быть больше нуля", nameof(amount));

        CurrentBalance += amount;
        TotalDeposited += amount;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Сумма списания должна быть больше нуля", nameof(amount));

        CurrentBalance -= amount;
        TotalSpent += amount;

        if (CurrentBalance < 0)
        {
            Debt = Math.Abs(CurrentBalance);
            CurrentBalance = 0;
        }

        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void ForgiveDebt()
    {
        Debt = 0;
        LastUpdated = DateTimeOffset.UtcNow;
    }
}
