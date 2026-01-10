namespace Billing.TimeCafe.Test.Integration.Repository.Transaction;

public class GetByIdAsyncTests : BaseTransactionRepositoryTest
{
    [Fact]
    public async Task Repository_GetById_Should_ReturnTransaction_WhenExists()
    {
        var createdTransaction = await CreateTestTransactionAsync();

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var result = await repository.GetByIdAsync(createdTransaction.TransactionId);

        result.Should().NotBeNull();
        result!.TransactionId.Should().Be(createdTransaction.TransactionId);
        result.UserId.Should().Be(DefaultsGuid.UserId);
        result.Amount.Should().Be(DefaultsGuid.DefaultAmount);
        result.Type.Should().Be(TransactionType.Deposit);
    }

    [Fact]
    public async Task Repository_GetById_Should_ReturnNull_WhenNotExists()
    {
        var nonExistentTransactionId = Guid.NewGuid();

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var result = await repository.GetByIdAsync(nonExistentTransactionId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetById_Should_ReturnFromCache_WhenCalledTwice()
    {
        await ClearCacheAsync();
        var createdTransaction = await CreateTestTransactionAsync();

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

        var result1 = await repository.GetByIdAsync(createdTransaction.TransactionId);
        var cacheKey = CacheKeys.Transaction_ById(createdTransaction.TransactionId);
        var cachedValue = await cache.GetStringAsync(cacheKey);
        cachedValue.Should().NotBeNullOrEmpty();

        var result2 = await repository.GetByIdAsync(createdTransaction.TransactionId);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1!.TransactionId.Should().Be(result2!.TransactionId);
    }

    [Fact]
    public async Task Repository_GetById_Should_ReturnEmptyGuid_AsValidId()
    {
        var emptyGuid = InvalidDataGuid.EmptyUserId;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var result = await repository.GetByIdAsync(emptyGuid);

        result.Should().BeNull();
    }
}
