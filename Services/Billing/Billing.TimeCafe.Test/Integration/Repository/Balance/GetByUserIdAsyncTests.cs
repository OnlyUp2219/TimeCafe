namespace Billing.TimeCafe.Test.Integration.Repository.Balance;

public class GetByIdAsyncTests : BaseBalanceRepositoryTest
{
    [Fact]
    public async Task Repository_GetByUserId_Should_ReturnBalance_WhenExists()
    {

        var userId = DefaultsGuid.UserId;
        await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result = await repository.GetByIdAsync(userId);

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

        var result = await repository.GetByIdAsync(nonExistentUserId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByUserId_Should_ReturnNull_WhenEmptyGuid()
    {

        var emptyUserId = InvalidDataGuid.EmptyUserId;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result = await repository.GetByIdAsync(emptyUserId);

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

        var result1 = await repository.GetByIdAsync(userId);
        var result2 = await repository.GetByIdAsync(userId);

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

        var result1 = await repository.GetByIdAsync(userId);
        result1!.CurrentBalance.Should().Be(DefaultsGuid.DefaultAmount);

        testBalance.CurrentBalance = DefaultsGuid.UpdatedAmount;
        testBalance.LastUpdated = DateTimeOffset.UtcNow;
        await repository.UpdateAsync(testBalance);
        await SaveAndInvalidateCacheAsync(scope, userId);

        var result2 = await repository.GetByIdAsync(userId);

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

        var result1 = await repository.GetByIdAsync(userId);

        var cacheKey = CacheKeys.Balance_ByUserId(userId);
        await scope.ServiceProvider.GetRequiredService<IDistributedCache>().RemoveAsync(cacheKey);

        var result2 = await repository.GetByIdAsync(userId);

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

        var result1 = await repository.GetByIdAsync(userId1);
        var result2 = await repository.GetByIdAsync(userId2);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.UserId.Should().Be(userId1);
        result2.UserId.Should().Be(userId2);

        var result1SecondCall = await repository.GetByIdAsync(userId1);
        var result2SecondCall = await repository.GetByIdAsync(userId2);

        result1SecondCall.Should().NotBeNull();
        result2SecondCall.Should().NotBeNull();
        result1SecondCall!.UserId.Should().Be(userId1);
        result2SecondCall!.UserId.Should().Be(userId2);
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

        var result1 = await repository.GetByIdAsync(userId1);
        var result2 = await repository.GetByIdAsync(userId2);
        var result3 = await repository.GetByIdAsync(nonExistentUserId);

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

        var result1 = await repository.GetByIdAsync(nonExistentUserId);
        var result2 = await repository.GetByIdAsync(nonExistentUserId);

        result1.Should().BeNull();
        result2.Should().BeNull();
    }
}
