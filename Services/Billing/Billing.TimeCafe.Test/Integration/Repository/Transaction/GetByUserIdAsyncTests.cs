namespace Billing.TimeCafe.Test.Integration.Repository.Transaction;

public class GetByUserIdAsyncTests : BaseTransactionRepositoryTest
{
    [Fact]
    public async Task Repository_GetByUserId_Should_ReturnTransactions_WhenExist()
    {
        await CreateTestTransactionAsync(Defaults.UserId, Defaults.DefaultAmount, TransactionType.Deposit);
        await CreateTestTransactionAsync(Defaults.UserId, Defaults.SmallAmount, TransactionType.Withdrawal);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var result = await repository.GetByUserIdAsync(Defaults.UserId, 1, 10);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(t => t.UserId.Should().Be(Defaults.UserId));
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_ReturnEmpty_WhenUserHasNoTransactions()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var result = await repository.GetByUserIdAsync(InvalidData.NonExistentUserId, 1, 10);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_ReturnOrderedByDescending()
    {
        var userIdToTest = Defaults.UserId;
        await CreateTestTransactionAsync(userIdToTest);
        await Task.Delay(100);
        await CreateTestTransactionAsync(userIdToTest);
        await Task.Delay(100);
        await CreateTestTransactionAsync(userIdToTest);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var result = await repository.GetByUserIdAsync(userIdToTest, 1, 10);

        result.Should().HaveCount(3);
        for (int i = 0; i < result.Count - 1; i++)
        {
            result[i].CreatedAt.Should().BeOnOrAfter(result[i + 1].CreatedAt);
        }
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_RespectPageSize()
    {
        var userIdToTest = Defaults.UserId;
        await CreateTestTransactionAsync(userIdToTest);
        await CreateTestTransactionAsync(userIdToTest);
        await CreateTestTransactionAsync(userIdToTest);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var result = await repository.GetByUserIdAsync(userIdToTest, 1, 2);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_RespectPageNumber()
    {
        var userIdToTest = Defaults.UserId;
        await CreateTestTransactionAsync(userIdToTest, Defaults.DefaultAmount);
        await Task.Delay(50);
        await CreateTestTransactionAsync(userIdToTest, Defaults.DefaultAmount);
        await Task.Delay(50);
        await CreateTestTransactionAsync(userIdToTest, Defaults.DefaultAmount);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var page1 = await repository.GetByUserIdAsync(userIdToTest, 1, 2);
        var page2 = await repository.GetByUserIdAsync(userIdToTest, 2, 2);

        page1.Should().HaveCount(2);
        page2.Should().HaveCount(1);
        page1[0].TransactionId.Should().NotBe(page2[0].TransactionId);
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_NotReturnOtherUsersTransactions()
    {
        await CreateTestTransactionAsync(Defaults.UserId);
        await CreateTestTransactionAsync(Defaults.UserId2);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var result = await repository.GetByUserIdAsync(Defaults.UserId, 1, 10);

        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(Defaults.UserId);
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_CacheResults()
    {
        await ClearCacheAsync();
        var userIdToTest = Defaults.UserId;
        await CreateTestTransactionAsync(userIdToTest);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

        var result1 = await repository.GetByUserIdAsync(userIdToTest, 1, 10);
        var cacheKey = CacheKeys.Transaction_History(userIdToTest, 1);
        var cachedValue = await cache.GetStringAsync(cacheKey);

        cachedValue.Should().NotBeNullOrEmpty();
        result1.Should().HaveCount(1);
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_ReturnEmpty_WhenPageOutOfRange()
    {
        var userIdToTest = Defaults.UserId;
        await CreateTestTransactionAsync(userIdToTest);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var result = await repository.GetByUserIdAsync(userIdToTest, 10, 10);

        result.Should().BeEmpty();
    }
}
