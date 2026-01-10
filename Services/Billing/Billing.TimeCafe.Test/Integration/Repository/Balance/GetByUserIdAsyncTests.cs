namespace Billing.TimeCafe.Test.Integration.Repository.Balance;

public class GetByUserIdAsyncTests : BaseBalanceRepositoryTest
{
    [Fact]
    public async Task Repository_GetByUserId_Should_ReturnBalance_WhenExists()
    {

        var userId = DefaultsGuid.UserId;
        await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result = await repository.GetByUserIdAsync(userId);

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.CurrentBalance.Should().Be(DefaultsGuid.DefaultAmount);
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_ReturnNull_WhenNotExists()
    {

        var nonExistentUserId = InvalidDataGuid.NonExistentUserId;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result = await repository.GetByUserIdAsync(nonExistentUserId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_ReturnNull_WhenEmptyGuid()
    {

        var emptyUserId = InvalidDataGuid.EmptyUserId;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result = await repository.GetByUserIdAsync(emptyUserId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_ReturnFromCache_WhenCalledTwice()
    {
        await ClearCacheAsync();
        var userId = DefaultsGuid.UserId;
        await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

        var result1 = await repository.GetByUserIdAsync(userId);
        var cacheKey = CacheKeys.Balance_ByUserId(userId);
        var cachedValue = await cache.GetStringAsync(cacheKey);
        cachedValue.Should().NotBeNullOrEmpty();

        var result2 = await repository.GetByUserIdAsync(userId);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.UserId.Should().Be(result2.UserId);
        result1.CurrentBalance.Should().Be(DefaultsGuid.DefaultAmount);
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_InvalidateCache_WhenUpdated()
    {
        await ClearCacheAsync();
        var userId = DefaultsGuid.UserId;
        var testBalance = await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result1 = await repository.GetByUserIdAsync(userId);
        result1!.CurrentBalance.Should().Be(DefaultsGuid.DefaultAmount);

        testBalance.CurrentBalance = DefaultsGuid.UpdatedAmount;
        testBalance.LastUpdated = DateTimeOffset.UtcNow;
        await repository.UpdateAsync(testBalance);

        var result2 = await repository.GetByUserIdAsync(userId);

        result2!.CurrentBalance.Should().Be(DefaultsGuid.UpdatedAmount);
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_RefetchFromDb_WhenCacheExpired()
    {
        await ClearCacheAsync();
        var userId = DefaultsGuid.UserId;
        await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

        var result1 = await repository.GetByUserIdAsync(userId);

        var cacheKey = CacheKeys.Balance_ByUserId(userId);
        await cache.RemoveAsync(cacheKey);

        var result2 = await repository.GetByUserIdAsync(userId);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result2.UserId.Should().Be(userId);
        result2.CurrentBalance.Should().Be(DefaultsGuid.DefaultAmount);
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_CacheMultipleUsers()
    {
        await ClearCacheAsync();
        var userId1 = DefaultsGuid.UserId;
        var userId2 = DefaultsGuid.UserId2;

        await CreateTestBalanceAsync(userId1);
        await CreateTestBalanceAsync(userId2);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

        var result1 = await repository.GetByUserIdAsync(userId1);
        var result2 = await repository.GetByUserIdAsync(userId2);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.UserId.Should().Be(userId1);
        result2.UserId.Should().Be(userId2);

        var cached1 = await cache.GetStringAsync(CacheKeys.Balance_ByUserId(userId1));
        var cached2 = await cache.GetStringAsync(CacheKeys.Balance_ByUserId(userId2));

        cached1.Should().NotBeNullOrEmpty();
        cached2.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_DifferentiateBetweenUsers()
    {
        await ClearCacheAsync();
        var userId1 = DefaultsGuid.UserId;
        var userId2 = DefaultsGuid.UserId2;
        var nonExistentUserId = InvalidDataGuid.NonExistentUserId;

        await CreateTestBalanceAsync(userId1, DefaultsGuid.DefaultAmount);
        await CreateTestBalanceAsync(userId2, DefaultsGuid.UpdatedAmount);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result1 = await repository.GetByUserIdAsync(userId1);
        var result2 = await repository.GetByUserIdAsync(userId2);
        var result3 = await repository.GetByUserIdAsync(nonExistentUserId);

        result1.Should().NotBeNull();
        result1.CurrentBalance.Should().Be(DefaultsGuid.DefaultAmount);

        result2.Should().NotBeNull();
        result2.CurrentBalance.Should().Be(DefaultsGuid.UpdatedAmount);

        result3.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_ReturnNullFromCache_WhenNotExists()
    {
        await ClearCacheAsync();
        var nonExistentUserId = InvalidDataGuid.NonExistentUserId;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result1 = await repository.GetByUserIdAsync(nonExistentUserId);
        var result2 = await repository.GetByUserIdAsync(nonExistentUserId);

        result1.Should().BeNull();
        result2.Should().BeNull();
    }
}
