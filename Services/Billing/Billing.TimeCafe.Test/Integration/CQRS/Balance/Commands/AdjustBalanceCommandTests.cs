namespace Billing.TimeCafe.Test.Integration.CQRS.Balance.Commands;

public class AdjustBalanceCommandTests : IDisposable
{
    protected HttpClient Client { get; }
    protected IntegrationApiFactory Factory { get; }

    public AdjustBalanceCommandTests()
    {
        Factory = new IntegrationApiFactory();
        Client = Factory.CreateClient();
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    [Fact]
    public async Task Command_AdjustBalance_Should_Deposit_IncreaseBalance_AndCreateTransaction()
    {
        var userId = DefaultsGuid.UserId;

        using (var scope = CreateScope())
        {
            var balances = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            await balances.CreateAsync(new BalanceModel(userId));
        }

        AdjustBalanceResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new AdjustBalanceCommand(userId, DefaultsGuid.SmallAmount, TransactionType.Deposit, TransactionSource.Manual));
        }

        result.Success.Should().BeTrue();
        result.Transaction!.Type.Should().Be(TransactionType.Deposit);
        result.Transaction.Amount.Should().Be(DefaultsGuid.SmallAmount);

        using var scope2 = CreateScope();
        var balances2 = scope2.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var fetched = await balances2.GetByUserIdAsync(userId);
        fetched!.CurrentBalance.Should().Be(DefaultsGuid.SmallAmount);
    }

    [Fact]
    public async Task Command_AdjustBalance_Should_Withdrawal_Block_WhenInsufficientFunds()
    {
        var userId = DefaultsGuid.UserId2;

        using (var scope = CreateScope())
        {
            var balances = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            await balances.CreateAsync(new BalanceModel(userId));
        }

        AdjustBalanceResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new AdjustBalanceCommand(userId, DefaultsGuid.DefaultAmount, TransactionType.Withdrawal, TransactionSource.Manual));
        }

        result.Success.Should().BeFalse();
        result.Code.Should().Be("InsufficientFunds");
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Command_AdjustBalance_Should_Withdrawal_DecreaseBalance_AndCreateTransaction()
    {
        var userId = DefaultsGuid.UserId3;

        using (var scope = CreateScope())
        {
            var balances = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            var balance = await balances.CreateAsync(new BalanceModel(userId));
            balance.CurrentBalance = DefaultsGuid.UpdatedAmount;
            balance.TotalDeposited = DefaultsGuid.UpdatedAmount;
            await balances.UpdateAsync(balance);
        }

        AdjustBalanceResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new AdjustBalanceCommand(userId, DefaultsGuid.SmallAmount, TransactionType.Withdrawal, TransactionSource.Manual));
        }

        result.Success.Should().BeTrue();
        result.Balance!.CurrentBalance.Should().Be(DefaultsGuid.UpdatedAmount - DefaultsGuid.SmallAmount);
        result.Transaction!.Type.Should().Be(TransactionType.Withdrawal);
        result.Transaction.Amount.Should().Be(DefaultsGuid.SmallAmount);
    }

    [Fact]
    public async Task Command_AdjustBalance_Should_ReturnDuplicate_WhenSourceIdAlreadyUsed()
    {
        var userId = DefaultsGuid.UserId;
        var sourceId = DefaultsGuid.TransactionId;

        using (var scope = CreateScope())
        {
            var balances = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            await balances.CreateAsync(new BalanceModel(userId));
        }

        AdjustBalanceResult first;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            first = await sender.Send(new AdjustBalanceCommand(userId, DefaultsGuid.SmallAmount, TransactionType.Deposit, TransactionSource.Payment, SourceId: sourceId));
        }
        first.Success.Should().BeTrue();

        AdjustBalanceResult second;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            second = await sender.Send(new AdjustBalanceCommand(userId, DefaultsGuid.SmallAmount, TransactionType.Deposit, TransactionSource.Payment, SourceId: sourceId));
        }

        second.Success.Should().BeFalse();
        second.Code.Should().Be("DuplicateTransaction");
        second.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Command_AdjustBalance_Should_ThrowValidationException_WhenInvalid()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new AdjustBalanceCommand(InvalidDataGuid.EmptyUserId, 0m, TransactionType.Deposit, TransactionSource.Manual));
        await action.Should().ThrowAsync<ValidationException>();
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
