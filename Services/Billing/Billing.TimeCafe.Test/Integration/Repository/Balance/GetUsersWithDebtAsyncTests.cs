namespace Billing.TimeCafe.Test.Integration.Repository.Balance;

using Billing.TimeCafe.Test.TestData;

public class GetUsersWithDebtAsyncTests : BaseBalanceRepositoryTest
{
    [Fact]
    public async Task Repository_GetUsersWithDebt_Should_ReturnEmpty_WhenNoDebtors()
    {

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result = await repository.GetUsersWithDebtAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetUsersWithDebt_Should_ReturnOnlyUsersWithDebt()
    {

        var debtorUserId = DefaultsGuid.UserId;
        var noDebtUserId = DefaultsGuid.UserId2;

        var balance1 = new BalanceModel(debtorUserId) { Debt = DefaultsGuid.DebtAmount };
        var balance2 = new BalanceModel(noDebtUserId) { Debt = 0m };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        await repository.CreateAsync(balance1);
        await repository.CreateAsync(balance2);

        var result = await repository.GetUsersWithDebtAsync();

        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(debtorUserId);
        result[0].Debt.Should().Be(DefaultsGuid.DebtAmount);
    }

    [Fact]
    public async Task Repository_GetUsersWithDebt_Should_OrderByDebtDescending()
    {

        var userId1 = DefaultsGuid.UserId;
        var userId2 = DefaultsGuid.UserId2;
        var userId3 = DefaultsGuid.UserId3;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        await repository.CreateAsync(new BalanceModel(userId1) { Debt = 30m });
        await repository.CreateAsync(new BalanceModel(userId2) { Debt = 100m });
        await repository.CreateAsync(new BalanceModel(userId3) { Debt = 50m });

        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        await cache.RemoveAsync(CacheKeys.Debtors_All);

        var result = await repository.GetUsersWithDebtAsync();

        result.Should().HaveCount(3);
        result[0].Debt.Should().Be(100m);
        result[1].Debt.Should().Be(50m);
        result[2].Debt.Should().Be(30m);
    }

    [Fact]
    public async Task Repository_GetUsersWithDebt_Should_ReturnFromCache_WhenCalledTwice()
    {

        var userId = DefaultsGuid.UserId;
        var balance = new BalanceModel(userId)
        {
            CurrentBalance = DefaultsGuid.SmallAmount,
            TotalDeposited = DefaultsGuid.SmallAmount,
            TotalSpent = DefaultsGuid.DefaultAmount,
            Debt = DefaultsGuid.DebtAmount
        };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        await repository.CreateAsync(balance);
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

        var result1 = await repository.GetUsersWithDebtAsync();
        var cachedValue = await cache.GetStringAsync(CacheKeys.Debtors_All);
        cachedValue.Should().NotBeNullOrEmpty();

        var result2 = await repository.GetUsersWithDebtAsync();

        result1.Should().HaveCount(1);
        result2.Should().HaveCount(1);
        result1[0].Debt.Should().Be(DefaultsGuid.DebtAmount);
    }

    [Fact]
    public async Task Repository_GetUsersWithDebt_Should_InvalidateCache_OnCreate()
    {
        await ClearCacheAsync();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var userId1 = DefaultsGuid.UserId;
        var userId2 = DefaultsGuid.UserId2;

        var balance1 = new BalanceModel(userId1) { Debt = DefaultsGuid.DebtAmount };
        await repository.CreateAsync(balance1);
        var result1 = await repository.GetUsersWithDebtAsync();

        var balance2 = new BalanceModel(userId2) { Debt = 75m };
        await repository.CreateAsync(balance2);
        var result2 = await repository.GetUsersWithDebtAsync();

        result1.Should().HaveCount(1);
        result2.Should().HaveCount(2);
    }

    [Fact]
    public async Task Repository_GetUsersWithDebt_Should_InvalidateCache_OnUpdate()
    {
        await ClearCacheAsync();
        var userId = DefaultsGuid.UserId;
        var balance = new BalanceModel(userId) { Debt = DefaultsGuid.DebtAmount };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        await repository.CreateAsync(balance);

        var result1 = await repository.GetUsersWithDebtAsync();
        result1.Should().HaveCount(1);
        result1[0].Debt.Should().Be(DefaultsGuid.DebtAmount);

        balance.Debt = DefaultsGuid.UpdatedAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;
        await repository.UpdateAsync(balance);

        var result2 = await repository.GetUsersWithDebtAsync();

        result2.Should().HaveCount(1);
        result2[0].Debt.Should().Be(DefaultsGuid.UpdatedAmount);
    }

    [Fact]
    public async Task Repository_GetUsersWithDebt_Should_ExcludeZeroDebt()
    {

        var userId1 = DefaultsGuid.UserId;
        var userId2 = DefaultsGuid.UserId2;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        await repository.CreateAsync(new BalanceModel(userId1) { Debt = DefaultsGuid.DebtAmount });
        await repository.CreateAsync(new BalanceModel(userId2) { Debt = 0m });

        var result = await repository.GetUsersWithDebtAsync();

        result.Should().HaveCount(1);
        result.Should().NotContain(b => b.UserId == userId2);
    }

    [Fact]
    public async Task Repository_GetUsersWithDebt_Should_HandleEmptyDatabase()
    {

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result1 = await repository.GetUsersWithDebtAsync();
        var result2 = await repository.GetUsersWithDebtAsync();

        result1.Should().BeEmpty();
        result2.Should().BeEmpty();
    }
}
