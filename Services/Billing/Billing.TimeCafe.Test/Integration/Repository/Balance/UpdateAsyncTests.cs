namespace Billing.TimeCafe.Test.Integration.Repository.Balance;

public class UpdateAsyncTests : BaseBalanceRepositoryTest
{
    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateBalance_WhenExists()
    {

        var userId = DefaultsGuid.UserId;
        var initialBalance = new BalanceModel(userId)
        {
            CurrentBalance = DefaultsGuid.DefaultAmount,
            TotalDeposited = DefaultsGuid.DefaultAmount
        };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var created = await repository.CreateAsync(initialBalance);
        await SaveAndInvalidateCacheAsync(scope, userId);

        created.CurrentBalance = DefaultsGuid.UpdatedAmount;
        created.TotalDeposited = DefaultsGuid.UpdatedAmount;
        created.LastUpdated = DateTimeOffset.UtcNow;

        var updated = await repository.UpdateAsync(created);
        await SaveAndInvalidateCacheAsync(scope, userId);

        updated.CurrentBalance.Should().Be(DefaultsGuid.UpdatedAmount);
        updated.TotalDeposited.Should().Be(DefaultsGuid.UpdatedAmount);

        var retrieved = await repository.GetByIdAsync(userId);
        retrieved!.CurrentBalance.Should().Be(DefaultsGuid.UpdatedAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateDebt_WhenDebtIncreases()
    {

        var userId = DefaultsGuid.UserId;
        var balance = new BalanceModel(userId)
        {
            CurrentBalance = 0m,
            Debt = DefaultsGuid.DebtAmount
        };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        await repository.CreateAsync(balance);
        await SaveAndInvalidateCacheAsync(scope, userId);

        balance.Debt = DefaultsGuid.UpdatedAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;
        var updated = await repository.UpdateAsync(balance);
        await SaveAndInvalidateCacheAsync(scope, userId);

        updated.Debt.Should().Be(DefaultsGuid.UpdatedAmount);

        var retrieved = await repository.GetByIdAsync(userId);
        retrieved!.Debt.Should().Be(DefaultsGuid.UpdatedAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_InvalidateCache()
    {

        var userId = DefaultsGuid.UserId;
        var balance = new BalanceModel(userId) { CurrentBalance = DefaultsGuid.DefaultAmount };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        await repository.CreateAsync(balance);
        await SaveAndInvalidateCacheAsync(scope, userId);
        var cached1 = await repository.GetByIdAsync(userId);

        balance.CurrentBalance = DefaultsGuid.UpdatedAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;
        await repository.UpdateAsync(balance);
        await SaveAndInvalidateCacheAsync(scope, userId);

        var retrieved = await repository.GetByIdAsync(userId);

        retrieved!.CurrentBalance.Should().Be(DefaultsGuid.UpdatedAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateLastUpdated()
    {

        var userId = DefaultsGuid.UserId;
        var balance = new BalanceModel(userId);
        var initialCreatedAt = balance.CreatedAt;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var created = await repository.CreateAsync(balance);

        created.CurrentBalance = DefaultsGuid.UpdatedAmount;
        var newLastUpdated = DateTimeOffset.UtcNow.AddHours(1);
        created.LastUpdated = newLastUpdated;

        var updated = await repository.UpdateAsync(created);

        updated.CreatedAt.Should().Be(initialCreatedAt);
        updated.LastUpdated.Should().BeCloseTo(newLastUpdated, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_HandleMultipleUpdates()
    {

        var userId = DefaultsGuid.UserId;
        var balance = new BalanceModel(userId) { CurrentBalance = 0m };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var created = await repository.CreateAsync(balance);
        await SaveAndInvalidateCacheAsync(scope, userId);

        created.CurrentBalance = DefaultsGuid.SmallAmount;
        created.LastUpdated = DateTimeOffset.UtcNow;
        var update1 = await repository.UpdateAsync(created);
        await SaveAndInvalidateCacheAsync(scope, userId);

        update1.CurrentBalance = DefaultsGuid.DefaultAmount;
        update1.LastUpdated = DateTimeOffset.UtcNow;
        var update2 = await repository.UpdateAsync(update1);
        await SaveAndInvalidateCacheAsync(scope, userId);

        update2.CurrentBalance = DefaultsGuid.UpdatedAmount;
        update2.LastUpdated = DateTimeOffset.UtcNow;
        var update3 = await repository.UpdateAsync(update2);
        await SaveAndInvalidateCacheAsync(scope, userId);

        update3.CurrentBalance.Should().Be(DefaultsGuid.UpdatedAmount);

        var final = await repository.GetByIdAsync(userId);
        final!.CurrentBalance.Should().Be(DefaultsGuid.UpdatedAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_HandleZeroBalance()
    {

        var userId = DefaultsGuid.UserId;
        var balance = await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        balance.CurrentBalance = 0m;
        balance.LastUpdated = DateTimeOffset.UtcNow;

        var result = await repository.UpdateAsync(balance);

        result.CurrentBalance.Should().Be(0m);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_HandleNegativeBalance()
    {

        var userId = DefaultsGuid.UserId;
        var balance = await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        balance.CurrentBalance = -DefaultsGuid.DefaultAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;

        var result = await repository.UpdateAsync(balance);

        result.CurrentBalance.Should().Be(-DefaultsGuid.DefaultAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_PreserveCreatedAt()
    {

        var userId = DefaultsGuid.UserId;
        var balance = await CreateTestBalanceAsync(userId);
        var originalCreatedAt = balance.CreatedAt;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        balance.CurrentBalance = DefaultsGuid.UpdatedAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow.AddHours(1);

        var result = await repository.UpdateAsync(balance);

        result.CreatedAt.Should().Be(originalCreatedAt);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_HandleDebtIncrease()
    {

        var userId = DefaultsGuid.UserId;
        var balance = await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        balance.Debt = DefaultsGuid.DebtAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;

        var result = await repository.UpdateAsync(balance);

        result.Debt.Should().Be(DefaultsGuid.DebtAmount);

        balance.Debt = DefaultsGuid.UpdatedAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;

        var result2 = await repository.UpdateAsync(balance);

        result2.Debt.Should().Be(DefaultsGuid.UpdatedAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_HandleDebtDecrease()
    {

        var userId = DefaultsGuid.UserId;
        var balance = await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        balance.Debt = DefaultsGuid.UpdatedAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;
        await repository.UpdateAsync(balance);

        balance.Debt = DefaultsGuid.DebtAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;

        var result = await repository.UpdateAsync(balance);

        result.Debt.Should().Be(DefaultsGuid.DebtAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_PersistChangesToDatabase()
    {

        var userId = DefaultsGuid.UserId;
        var balance = await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        balance.CurrentBalance = DefaultsGuid.UpdatedAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;

        await repository.UpdateAsync(balance);
        await SaveAndInvalidateCacheAsync(scope, userId);

        var retrieved = await repository.GetByIdAsync(userId);
        retrieved.Should().NotBeNull();
        retrieved.CurrentBalance.Should().Be(DefaultsGuid.UpdatedAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_HandleAllPropertiesUpdate()
    {

        var userId = DefaultsGuid.UserId;
        var balance = await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        balance.CurrentBalance = DefaultsGuid.UpdatedAmount;
        balance.TotalDeposited = DefaultsGuid.UpdatedAmount + DefaultsGuid.DefaultAmount;
        balance.TotalSpent = DefaultsGuid.DefaultAmount;
        balance.Debt = DefaultsGuid.DebtAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;

        var result = await repository.UpdateAsync(balance);

        result.CurrentBalance.Should().Be(DefaultsGuid.UpdatedAmount);
        result.TotalDeposited.Should().Be(DefaultsGuid.UpdatedAmount + DefaultsGuid.DefaultAmount);
        result.TotalSpent.Should().Be(DefaultsGuid.DefaultAmount);
        result.Debt.Should().Be(DefaultsGuid.DebtAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_HandleLargeAmount()
    {

        var userId = DefaultsGuid.UserId;
        var balance = await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        const decimal largeAmount = 999999999.99m;
        balance.CurrentBalance = largeAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;

        var result = await repository.UpdateAsync(balance);

        result.CurrentBalance.Should().Be(largeAmount);
    }
}
