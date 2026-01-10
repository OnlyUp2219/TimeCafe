namespace Billing.TimeCafe.Test.Integration.Repository.Balance;

public class ExistsAsyncTests : BaseBalanceRepositoryTest
{
    [Fact]
    public async Task Repository_ExistsAsync_Should_ReturnTrue_WhenBalanceExists()
    {

        var userId = DefaultsGuid.UserId;
        var balance = await CreateTestBalanceAsync(userId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var exists = await repository.ExistsAsync(userId);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_ExistsAsync_Should_ReturnFalse_WhenBalanceNotExists()
    {

        var nonExistentUserId = DefaultsGuid.UserId2;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var exists = await repository.ExistsAsync(nonExistentUserId);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_ExistsAsync_Should_CheckMultipleUsers()
    {

        var userId1 = DefaultsGuid.UserId;
        var userId2 = DefaultsGuid.UserId2;
        var nonExistentUserId = DefaultsGuid.UserId3;

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        await CreateTestBalanceAsync(userId1);
        await CreateTestBalanceAsync(userId2);

        var exists1 = await repository.ExistsAsync(userId1);
        var exists2 = await repository.ExistsAsync(userId2);
        var existsNone = await repository.ExistsAsync(nonExistentUserId);

        exists1.Should().BeTrue();
        exists2.Should().BeTrue();
        existsNone.Should().BeFalse();
    }
}
