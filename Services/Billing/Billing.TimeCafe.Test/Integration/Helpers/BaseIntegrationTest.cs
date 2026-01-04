namespace Billing.TimeCafe.Test.Integration.Helpers;

public abstract class BaseIntegrationTest(IntegrationApiFactory factory) : IClassFixture<IntegrationApiFactory>
{
    protected readonly IntegrationApiFactory Factory = factory;
    protected readonly HttpClient Client = factory.CreateClient();

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    protected async Task<Balance> SeedBalanceAsync(Guid userId, decimal amount = 0m)
    {
        using var scope = CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var existing = await ctx.Balances.FindAsync(userId);
        if (existing != null)
        {
            existing.CurrentBalance = amount;
            await ctx.SaveChangesAsync();
            return existing;
        }

        var balance = new Balance
        {
            UserId = userId,
            CurrentBalance = amount,
            TotalDeposited = amount,
            LastUpdated = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };

        ctx.Balances.Add(balance);
        await ctx.SaveChangesAsync();
        return balance;
    }

    protected async Task<Transaction> SeedTransactionAsync(Guid userId, decimal amount, TransactionType type, TransactionSource source, Guid? sourceId = null)
    {
        using var scope = CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            Type = type,
            Source = source,
            SourceId = sourceId,
            Status = TransactionStatus.Completed,
            CreatedAt = DateTimeOffset.UtcNow,
            BalanceAfter = amount
        };
        ctx.Transactions.Add(transaction);
        await ctx.SaveChangesAsync();
        return transaction;
    }

    protected async Task<Payment> SeedPaymentAsync(Guid userId, decimal amount, PaymentStatus status)
    {
        using var scope = CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            PaymentMethod = PaymentMethod.Online,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow
        };
        ctx.Payments.Add(payment);
        await ctx.SaveChangesAsync();
        return payment;
    }
}
