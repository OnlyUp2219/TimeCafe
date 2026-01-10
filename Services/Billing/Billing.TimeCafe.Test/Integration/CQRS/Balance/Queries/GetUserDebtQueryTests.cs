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
        var userId = Defaults.UserId3;

        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var before = await db.Balances.FindAsync(Guid.Parse(userId));
        before.Should().BeNull();

        var result = await sender.Send(new GetUserDebtQuery(userId));

        result.Success.Should().BeTrue();
        result.Debt.Should().Be(0m);

        var created = await db.Balances.FindAsync(Guid.Parse(userId));
        created.Should().NotBeNull();
        created!.UserId.Should().Be(Guid.Parse(userId));
        created.Debt.Should().Be(0m);
    }

    [Fact]
    public async Task Query_GetUserDebt_Should_ReturnDebt_WhenDebtExists()
    {
        var userId = Defaults.UserId;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var balance = await repository.CreateAsync(new BalanceModel(userId));
        balance.Deposit(Defaults.SmallAmount);
        balance.Withdraw(Defaults.DefaultAmount);
        await repository.UpdateAsync(balance);

        var result = await sender.Send(new GetUserDebtQuery(userId));

        result.Success.Should().BeTrue();
        result.Debt.Should().Be(balance.Debt);
    }

    [Fact]
    public async Task Query_GetUserDebt_Should_ReturnZero_WhenNoDebt()
    {
        var userId = Defaults.UserId2;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var balance = new BalanceModel(userId) { CurrentBalance = Defaults.DefaultAmount };
        await repository.CreateAsync(balance);

        var result = await sender.Send(new GetUserDebtQuery(userId));

        result.Success.Should().BeTrue();
        result.Debt.Should().Be(0m);
    }

    [Fact]
    public async Task Query_GetUserDebt_Should_ThrowValidationException_WhenUserIdEmpty()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetUserDebtQuery(InvalidData.EmptyUserId));

        await action.Should().ThrowAsync<ValidationException>();
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
