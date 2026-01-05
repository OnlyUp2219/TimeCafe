namespace Billing.TimeCafe.Test.Integration.Repository.Balance;

using Billing.TimeCafe.Test.TestData;

public class CreateAsyncTests : BaseBalanceRepositoryTest
{
    [Fact]
    public async Task Repository_CreateAsync_Should_InsertBalance_WhenValid()
    {

        var userId = Defaults.UserId;
        var balance = new BalanceModel(userId)
        {
            CurrentBalance = Defaults.DefaultAmount,
            TotalDeposited = Defaults.DefaultAmount
        };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result = await repository.CreateAsync(balance);

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.CurrentBalance.Should().Be(Defaults.DefaultAmount);
        result.TotalDeposited.Should().Be(Defaults.DefaultAmount);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_ReturnExisting_WhenAlreadyExists()
    {

        var userId = Defaults.UserId;
        var balance = new BalanceModel(userId) { CurrentBalance = Defaults.DefaultAmount };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var created = await repository.CreateAsync(balance);
        var existing = await repository.CreateAsync(new BalanceModel(userId) { CurrentBalance = Defaults.UpdatedAmount });

        created.UserId.Should().Be(userId);
        existing.UserId.Should().Be(userId);
        existing.CurrentBalance.Should().Be(Defaults.DefaultAmount);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_InvalidateCache()
    {

        var userId = Defaults.UserId;
        var balance = new BalanceModel(userId) { CurrentBalance = Defaults.DefaultAmount };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        await repository.CreateAsync(balance);

        var retrieved = await repository.GetByUserIdAsync(userId);

        retrieved.Should().NotBeNull();
        retrieved.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_HandleConcurrentCreation()
    {

        var userId = Defaults.UserId;
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var balance1 = new BalanceModel(userId) { CurrentBalance = Defaults.DefaultAmount };
        var balance2 = new BalanceModel(userId) { CurrentBalance = Defaults.UpdatedAmount };

        var task1 = repository.CreateAsync(balance1);
        var task2 = repository.CreateAsync(balance2);

        await Task.WhenAll(task1, task2);

        var result1 = task1.Result;
        var result2 = task2.Result;

        result1.UserId.Should().Be(userId);
        result2.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_CreateWithZeroAmount()
    {

        var userId = Defaults.UserId;
        var balance = new BalanceModel(userId)
        {
            CurrentBalance = 0m,
            TotalDeposited = 0m
        };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result = await repository.CreateAsync(balance);

        result.Should().NotBeNull();
        result.CurrentBalance.Should().Be(0m);
        result.TotalDeposited.Should().Be(0m);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_CreateWithNegativeAmount()
    {

        var userId = Defaults.UserId;
        var balance = new BalanceModel(userId)
        {
            CurrentBalance = -Defaults.DefaultAmount,
            Debt = Defaults.DefaultAmount
        };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result = await repository.CreateAsync(balance);

        result.CurrentBalance.Should().Be(-Defaults.DefaultAmount);
        result.Debt.Should().Be(Defaults.DefaultAmount);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_PreserveAllProperties()
    {

        var userId = Defaults.UserId;
        var balance = new BalanceModel(userId)
        {
            CurrentBalance = Defaults.DefaultAmount,
            TotalDeposited = Defaults.DefaultAmount,
            TotalSpent = Defaults.SmallAmount,
            Debt = Defaults.DebtAmount
        };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result = await repository.CreateAsync(balance);

        result.CurrentBalance.Should().Be(Defaults.DefaultAmount);
        result.TotalDeposited.Should().Be(Defaults.DefaultAmount);
        result.TotalSpent.Should().Be(Defaults.SmallAmount);
        result.Debt.Should().Be(Defaults.DebtAmount);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_SetTimestamps()
    {

        var userId = Defaults.UserId;
        var beforeCreate = DateTimeOffset.UtcNow;
        var balance = new BalanceModel(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result = await repository.CreateAsync(balance);

        result.CreatedAt.Should().BeOnOrAfter(beforeCreate);
        result.LastUpdated.Should().BeOnOrAfter(beforeCreate);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_NotCreateDuplicate()
    {

        var userId = Defaults.UserId;
        var balance1 = new BalanceModel(userId) { CurrentBalance = Defaults.DefaultAmount };
        var balance2 = new BalanceModel(userId) { CurrentBalance = Defaults.UpdatedAmount };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var result1 = await repository.CreateAsync(balance1);
        var result2 = await repository.CreateAsync(balance2);

        result1.CurrentBalance.Should().Be(Defaults.DefaultAmount);
        result2.CurrentBalance.Should().Be(Defaults.DefaultAmount);
        result1.UserId.Should().Be(result2.UserId);

        using var dbScope = CreateScope(); var db = dbScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var count = db.Balances.Count(b => b.UserId == userId);
        count.Should().Be(1);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_HandleLargeAmount()
    {

        var userId = Defaults.UserId;
        var largeAmount = 999999999.99m;
        var balance = new BalanceModel(userId) { CurrentBalance = largeAmount };

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var result = await repository.CreateAsync(balance);

        result.Should().NotBeNull();
        result.CurrentBalance.Should().Be(largeAmount);
    }
}
