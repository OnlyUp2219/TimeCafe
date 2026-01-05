namespace Billing.TimeCafe.Test.Integration.Repository.Balance;

using Billing.TimeCafe.Test.TestData;

public class UpdateAsyncTests : BaseBalanceRepositoryTest
{
    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateBalance_WhenExists()
    {

        var userId = Defaults.UserId;
        var initialBalance = new BalanceModel(userId)
        {
            CurrentBalance = Defaults.DefaultAmount,
            TotalDeposited = Defaults.DefaultAmount
        };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var created = await repository.CreateAsync(initialBalance);

        created.CurrentBalance = Defaults.UpdatedAmount;
        created.TotalDeposited = Defaults.UpdatedAmount;
        created.LastUpdated = DateTimeOffset.UtcNow;

        var updated = await repository.UpdateAsync(created);

        updated.CurrentBalance.Should().Be(Defaults.UpdatedAmount);
        updated.TotalDeposited.Should().Be(Defaults.UpdatedAmount);

        var retrieved = await repository.GetByUserIdAsync(userId);
        retrieved.CurrentBalance.Should().Be(Defaults.UpdatedAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateDebt_WhenDebtIncreases()
    {

        var userId = Defaults.UserId;
        var balance = new BalanceModel(userId)
        {
            CurrentBalance = 0m,
            Debt = Defaults.DebtAmount
        };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        await repository.CreateAsync(balance);

        balance.Debt = Defaults.UpdatedAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;
        var updated = await repository.UpdateAsync(balance);

        updated.Debt.Should().Be(Defaults.UpdatedAmount);

        var retrieved = await repository.GetByUserIdAsync(userId);
        retrieved.Debt.Should().Be(Defaults.UpdatedAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_InvalidateCache()
    {

        var userId = Defaults.UserId;
        var balance = new BalanceModel(userId) { CurrentBalance = Defaults.DefaultAmount };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        await repository.CreateAsync(balance);
        var cached1 = await repository.GetByUserIdAsync(userId);

        balance.CurrentBalance = Defaults.UpdatedAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;
        await repository.UpdateAsync(balance);

        var retrieved = await repository.GetByUserIdAsync(userId);

        retrieved.CurrentBalance.Should().Be(Defaults.UpdatedAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateLastUpdated()
    {

        var userId = Defaults.UserId;
        var balance = new BalanceModel(userId);
        var initialCreatedAt = balance.CreatedAt;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var created = await repository.CreateAsync(balance);

        created.CurrentBalance = Defaults.UpdatedAmount;
        var newLastUpdated = DateTimeOffset.UtcNow.AddHours(1);
        created.LastUpdated = newLastUpdated;

        var updated = await repository.UpdateAsync(created);

        updated.CreatedAt.Should().Be(initialCreatedAt);
        updated.LastUpdated.Should().BeCloseTo(newLastUpdated, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_HandleMultipleUpdates()
    {

        var userId = Defaults.UserId;
        var balance = new BalanceModel(userId) { CurrentBalance = 0m };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var created = await repository.CreateAsync(balance);

        created.CurrentBalance = Defaults.SmallAmount;
        created.LastUpdated = DateTimeOffset.UtcNow;
        var update1 = await repository.UpdateAsync(created);

        update1.CurrentBalance = Defaults.DefaultAmount;
        update1.LastUpdated = DateTimeOffset.UtcNow;
        var update2 = await repository.UpdateAsync(update1);

        update2.CurrentBalance = Defaults.UpdatedAmount;
        update2.LastUpdated = DateTimeOffset.UtcNow;
        var update3 = await repository.UpdateAsync(update2);

        update3.CurrentBalance.Should().Be(Defaults.UpdatedAmount);

        var final = await repository.GetByUserIdAsync(userId);
        final.CurrentBalance.Should().Be(Defaults.UpdatedAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_HandleZeroBalance()
    {

        var userId = Defaults.UserId;
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

        var userId = Defaults.UserId;
        var balance = await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        balance.CurrentBalance = -Defaults.DefaultAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;

        var result = await repository.UpdateAsync(balance);

        result.CurrentBalance.Should().Be(-Defaults.DefaultAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_PreserveCreatedAt()
    {

        var userId = Defaults.UserId;
        var balance = await CreateTestBalanceAsync(userId);
        var originalCreatedAt = balance.CreatedAt;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        balance.CurrentBalance = Defaults.UpdatedAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow.AddHours(1);

        var result = await repository.UpdateAsync(balance);

        result.CreatedAt.Should().Be(originalCreatedAt);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_HandleDebtIncrease()
    {

        var userId = Defaults.UserId;
        var balance = await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        balance.Debt = Defaults.DebtAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;

        var result = await repository.UpdateAsync(balance);

        result.Debt.Should().Be(Defaults.DebtAmount);

        balance.Debt = Defaults.UpdatedAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;

        var result2 = await repository.UpdateAsync(balance);

        result2.Debt.Should().Be(Defaults.UpdatedAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_HandleDebtDecrease()
    {

        var userId = Defaults.UserId;
        var balance = await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        balance.Debt = Defaults.UpdatedAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;
        await repository.UpdateAsync(balance);

        balance.Debt = Defaults.DebtAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;

        var result = await repository.UpdateAsync(balance);

        result.Debt.Should().Be(Defaults.DebtAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_PersistChangesToDatabase()
    {

        var userId = Defaults.UserId;
        var balance = await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        balance.CurrentBalance = Defaults.UpdatedAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;

        await repository.UpdateAsync(balance);

        var retrieved = await repository.GetByUserIdAsync(userId);
        retrieved.Should().NotBeNull();
        retrieved.CurrentBalance.Should().Be(Defaults.UpdatedAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_HandleAllPropertiesUpdate()
    {

        var userId = Defaults.UserId;
        var balance = await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        balance.CurrentBalance = Defaults.UpdatedAmount;
        balance.TotalDeposited = Defaults.UpdatedAmount + Defaults.DefaultAmount;
        balance.TotalSpent = Defaults.DefaultAmount;
        balance.Debt = Defaults.DebtAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;

        var result = await repository.UpdateAsync(balance);

        result.CurrentBalance.Should().Be(Defaults.UpdatedAmount);
        result.TotalDeposited.Should().Be(Defaults.UpdatedAmount + Defaults.DefaultAmount);
        result.TotalSpent.Should().Be(Defaults.DefaultAmount);
        result.Debt.Should().Be(Defaults.DebtAmount);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_HandleLargeAmount()
    {

        var userId = Defaults.UserId;
        var balance = await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var largeAmount = 999999999.99m;
        balance.CurrentBalance = largeAmount;
        balance.LastUpdated = DateTimeOffset.UtcNow;

        var result = await repository.UpdateAsync(balance);

        result.CurrentBalance.Should().Be(largeAmount);
    }
}
