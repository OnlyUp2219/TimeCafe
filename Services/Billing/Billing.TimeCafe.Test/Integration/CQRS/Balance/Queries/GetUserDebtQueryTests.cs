using FluentResults;

namespace Billing.TimeCafe.Test.Integration.CQRS.Balance.Queries;

public class GetUserDebtQueryTests : IDisposable
{
    protected HttpClient Client { get; }
    protected IntegrationApiFactory Factory { get; }

    public GetUserDebtQueryTests()
    {
        Factory = new IntegrationApiFactory();
        Client = Factory.CreateClient();
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    [Fact]
    public async Task Query_GetUserDebt_Should_CreateBalance_WhenNotExists()
    {
        var userId = DefaultsGuid.UserId3;

        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var before = await db.Balances.FindAsync(userId);
        before.Should().BeNull();

        var result = await sender.Send(new GetUserDebtQuery(userId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Debt.Should().Be(0m);

        var created = await db.Balances.FindAsync(userId);
        created.Should().NotBeNull();
        created!.UserId.Should().Be(userId);
        created.Debt.Should().Be(0m);
    }

    [Fact]
    public async Task Query_GetUserDebt_Should_ReturnDebt_WhenDebtExists()
    {
        var userId = DefaultsGuid.UserId;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var balance = await repository.CreateAsync(new BalanceModel(userId));
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.SaveChangesAsync();
        balance.Deposit(DefaultsGuid.SmallAmount);
        balance.Withdraw(DefaultsGuid.DefaultAmount);
        await repository.UpdateAsync(balance);
        await db.SaveChangesAsync();

        var result = await sender.Send(new GetUserDebtQuery(userId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Debt.Should().Be(balance.Debt);
    }

    [Fact]
    public async Task Query_GetUserDebt_Should_ReturnZero_WhenNoDebt()
    {
        var userId = DefaultsGuid.UserId2;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var balance = new BalanceModel(userId) { CurrentBalance = DefaultsGuid.DefaultAmount };
        await repository.CreateAsync(balance);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.SaveChangesAsync();

        var result = await sender.Send(new GetUserDebtQuery(userId));

        result.IsSuccess.Should().BeTrue();
        result.Value.Debt.Should().Be(0m);
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
