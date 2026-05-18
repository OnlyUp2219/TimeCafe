using FluentValidation;
using FluentResults;

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
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.SaveChangesAsync();
        }

        Result<AdjustBalanceResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new AdjustBalanceCommand(userId, DefaultsGuid.SmallAmount, TransactionType.Deposit, TransactionSource.Manual));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.TransactionType.Should().Be(TransactionType.Deposit.ToString());
        result.Value.TransactionAmount.Should().Be(DefaultsGuid.SmallAmount);

        using var scope2 = CreateScope();
        var balances2 = scope2.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var fetched = await balances2.GetByIdAsync(userId);
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
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.SaveChangesAsync();
        }

        Result<AdjustBalanceResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new AdjustBalanceCommand(userId, DefaultsGuid.DefaultAmount, TransactionType.Withdrawal, TransactionSource.Manual));
        }

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Metadata.Contains(new KeyValuePair<string, object>("ErrorCode", "400")));
    }

    [Fact]
    public async Task Command_AdjustBalance_Should_Withdrawal_DecreaseBalance_AndCreateTransaction()
    {
        var userId = DefaultsGuid.UserId3;

        using (var scope = CreateScope())
        {
            var balances = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            var balance = new BalanceModel(userId)
            {
                CurrentBalance = DefaultsGuid.UpdatedAmount,
                TotalDeposited = DefaultsGuid.UpdatedAmount,
                Debt = 0m
            };
            await balances.CreateAsync(balance);
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.SaveChangesAsync();
        }

        Result<AdjustBalanceResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new AdjustBalanceCommand(userId, DefaultsGuid.SmallAmount, TransactionType.Withdrawal, TransactionSource.Manual));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.CurrentBalance.Should().Be(DefaultsGuid.UpdatedAmount - DefaultsGuid.SmallAmount);
        result.Value.TransactionType.Should().Be(TransactionType.Withdrawal.ToString());
        result.Value.TransactionAmount.Should().Be(-DefaultsGuid.SmallAmount);
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
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.SaveChangesAsync();
        }

        Result<AdjustBalanceResponse> first;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            first = await sender.Send(new AdjustBalanceCommand(userId, DefaultsGuid.SmallAmount, TransactionType.Deposit, TransactionSource.Payment, SourceId: sourceId));
        }
        first.IsSuccess.Should().BeTrue();

        Result<AdjustBalanceResponse> second;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            second = await sender.Send(new AdjustBalanceCommand(userId, DefaultsGuid.SmallAmount, TransactionType.Deposit, TransactionSource.Payment, SourceId: sourceId));
        }

        second.IsFailed.Should().BeTrue();
        second.Errors.Should().ContainSingle(e => e.Metadata.Contains(new KeyValuePair<string, object>("ErrorCode", "409")));
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
