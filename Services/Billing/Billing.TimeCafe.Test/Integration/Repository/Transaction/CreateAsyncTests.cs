namespace Billing.TimeCafe.Test.Integration.Repository.Transaction;

public class CreateAsyncTests : BaseTransactionRepositoryTest
{
    [Fact]
    public async Task Repository_CreateAsync_Should_InsertTransaction_WhenValid()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var transaction = TransactionModel.CreateDeposit(
            Defaults.UserId,
            Defaults.DefaultAmount,
            TransactionSource.Manual);

        var result = await repository.CreateAsync(transaction);

        result.Should().NotBeNull();
        result.TransactionId.Should().NotBe(Guid.Empty);
        result.UserId.Should().Be(Defaults.UserId);
        result.Amount.Should().Be(Defaults.DefaultAmount);
        result.Type.Should().Be(TransactionType.Deposit);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_CreateWithdrawal()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var transaction = TransactionModel.CreateWithdrawal(
            Defaults.UserId,
            Defaults.SmallAmount,
            TransactionSource.Visit,
            Defaults.TariffId);

        var result = await repository.CreateAsync(transaction);

        result.Should().NotBeNull();
        result.Type.Should().Be(TransactionType.Withdrawal);
        result.Amount.Should().Be(-Defaults.SmallAmount);
        result.Source.Should().Be(TransactionSource.Visit);
        result.SourceId.Should().Be(Defaults.TariffId);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_SetBalanceAfter()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var transaction = TransactionModel.CreateDeposit(
            Defaults.UserId,
            Defaults.DefaultAmount,
            TransactionSource.Payment);
        transaction.BalanceAfter = Defaults.DefaultAmount;

        var result = await repository.CreateAsync(transaction);

        result.BalanceAfter.Should().Be(Defaults.DefaultAmount);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_InvalidateUserHistoryCache()
    {
        await ClearCacheAsync();
        var transaction1 = await CreateTestTransactionAsync();

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var cacheKey = CacheKeys.Transaction_History(Defaults.UserId, 1);
        var cachedBefore = await scope.ServiceProvider
            .GetRequiredService<IDistributedCache>()
            .GetStringAsync(cacheKey);

        var transaction2 = TransactionModel.CreateDeposit(
            Defaults.UserId,
            Defaults.SmallAmount,
            TransactionSource.Manual);

        await repository.CreateAsync(transaction2);

        var cachedAfter = await scope.ServiceProvider
            .GetRequiredService<IDistributedCache>()
            .GetStringAsync(cacheKey);

        cachedBefore.Should().BeNull();
        cachedAfter.Should().BeNull();
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_AllowMultipleTransactionsForSameUser()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var transaction1 = TransactionModel.CreateDeposit(Defaults.UserId, Defaults.DefaultAmount, TransactionSource.Manual);
        var transaction2 = TransactionModel.CreateWithdrawal(Defaults.UserId, Defaults.SmallAmount, TransactionSource.Visit, Defaults.TariffId);

        var result1 = await repository.CreateAsync(transaction1);
        var result2 = await repository.CreateAsync(transaction2);

        result1.TransactionId.Should().NotBe(result2.TransactionId);
        result1.UserId.Should().Be(result2.UserId);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_AllowSameSourceIdForDifferentUsers()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var transaction1 = TransactionModel.CreateDeposit(
            Defaults.UserId,
            Defaults.DefaultAmount,
            TransactionSource.Visit,
            Defaults.TariffId);

        var transaction2 = TransactionModel.CreateDeposit(
            Defaults.UserId2,
            Defaults.DefaultAmount,
            TransactionSource.Visit,
            Defaults.TariffId);

        var result1 = await repository.CreateAsync(transaction1);
        var result2 = await repository.CreateAsync(transaction2);

        result1.SourceId.Should().Be(result2.SourceId);
        result1.UserId.Should().NotBe(result2.UserId);
    }
}
