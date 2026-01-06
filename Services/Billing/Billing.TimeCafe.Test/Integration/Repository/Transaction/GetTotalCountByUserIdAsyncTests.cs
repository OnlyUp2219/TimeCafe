namespace Billing.TimeCafe.Test.Integration.Repository.Transaction;

public class GetTotalCountByUserIdAsyncTests : BaseTransactionRepositoryTest
{
    [Fact]
    public async Task Repository_GetTotalCount_Should_ReturnZero_WhenUserHasNoTransactions()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var count = await repository.GetTotalCountByUserIdAsync(InvalidData.NonExistentUserId);

        count.Should().Be(0);
    }

    [Fact]
    public async Task Repository_GetTotalCount_Should_CountAllTransactions()
    {
        var userIdToTest = Defaults.UserId;
        await CreateTestTransactionAsync(userIdToTest, Defaults.DefaultAmount, TransactionType.Deposit);
        await CreateTestTransactionAsync(userIdToTest, Defaults.SmallAmount, TransactionType.Withdrawal);
        await CreateTestTransactionAsync(userIdToTest, Defaults.SmallAmount, TransactionType.Withdrawal);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var count = await repository.GetTotalCountByUserIdAsync(userIdToTest);

        count.Should().Be(3);
    }

    [Fact]
    public async Task Repository_GetTotalCount_Should_CountOnlySpecificUser()
    {
        var userIdToTest = Defaults.UserId;
        await CreateTestTransactionAsync(userIdToTest);
        await CreateTestTransactionAsync(userIdToTest);
        await CreateTestTransactionAsync(Defaults.UserId2);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var countUser1 = await repository.GetTotalCountByUserIdAsync(userIdToTest);
        var countUser2 = await repository.GetTotalCountByUserIdAsync(Defaults.UserId2);

        countUser1.Should().Be(2);
        countUser2.Should().Be(1);
    }

    [Fact]
    public async Task Repository_GetTotalCount_Should_CountDifferentTypes()
    {
        var userIdToTest = Defaults.UserId;
        await CreateTestTransactionAsync(userIdToTest, Defaults.DefaultAmount, TransactionType.Deposit);
        await CreateTestTransactionAsync(userIdToTest, Defaults.SmallAmount, TransactionType.Withdrawal);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var count = await repository.GetTotalCountByUserIdAsync(userIdToTest);

        count.Should().Be(2);
    }

    [Fact]
    public async Task Repository_GetTotalCount_Should_CountDifferentSources()
    {
        var userIdToTest = Defaults.UserId;
        await CreateTestTransactionAsync(userIdToTest, Defaults.DefaultAmount, TransactionType.Deposit, TransactionSource.Manual);
        await CreateTestTransactionAsync(userIdToTest, Defaults.SmallAmount, TransactionType.Withdrawal, TransactionSource.Visit, Defaults.TariffId);
        await CreateTestTransactionAsync(userIdToTest, Defaults.SmallAmount, TransactionType.Deposit, TransactionSource.Refund);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var count = await repository.GetTotalCountByUserIdAsync(userIdToTest);

        count.Should().Be(3);
    }

    [Fact]
    public async Task Repository_GetTotalCount_Should_IncrementAfterCreate()
    {
        var userIdToTest = Defaults.UserId;
        await CreateTestTransactionAsync(userIdToTest);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var countBefore = await repository.GetTotalCountByUserIdAsync(userIdToTest);
        await CreateTestTransactionAsync(userIdToTest);
        var countAfter = await repository.GetTotalCountByUserIdAsync(userIdToTest);

        countBefore.Should().Be(1);
        countAfter.Should().Be(2);
    }
}
