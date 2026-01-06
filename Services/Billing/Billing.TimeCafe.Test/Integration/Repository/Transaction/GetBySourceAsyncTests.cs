namespace Billing.TimeCafe.Test.Integration.Repository.Transaction;

public class GetBySourceAsyncTests : BaseTransactionRepositoryTest
{
    [Fact]
    public async Task Repository_GetBySource_Should_ReturnTransactions_WhenExist()
    {
        var visitId = Defaults.TariffId;
        await CreateTestTransactionAsync(Defaults.UserId, Defaults.SmallAmount, TransactionType.Withdrawal, TransactionSource.Visit, visitId);
        await CreateTestTransactionAsync(Defaults.UserId2, Defaults.SmallAmount, TransactionType.Withdrawal, TransactionSource.Visit, visitId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var result = await repository.GetBySourceAsync(TransactionSource.Visit, visitId);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(t => t.Source.Should().Be(TransactionSource.Visit));
        result.Should().AllSatisfy(t => t.SourceId.Should().Be(visitId));
    }

    [Fact]
    public async Task Repository_GetBySource_Should_ReturnEmpty_WhenSourceNotExists()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var result = await repository.GetBySourceAsync(TransactionSource.Visit, Guid.NewGuid());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetBySource_Should_ReturnEmpty_WhenSourceTypeNotMatches()
    {
        var visitId = Defaults.TariffId;
        await CreateTestTransactionAsync(Defaults.UserId, Defaults.SmallAmount, TransactionType.Withdrawal, TransactionSource.Visit, visitId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var result = await repository.GetBySourceAsync(TransactionSource.Payment, visitId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetBySource_Should_ReturnOrderedByAscending()
    {
        var paymentId = Defaults.TariffId;
        await CreateTestTransactionAsync(Defaults.UserId, Defaults.DefaultAmount, TransactionType.Deposit, TransactionSource.Payment, paymentId);
        await Task.Delay(100);
        await CreateTestTransactionAsync(Defaults.UserId2, Defaults.DefaultAmount, TransactionType.Deposit, TransactionSource.Payment, paymentId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var result = await repository.GetBySourceAsync(TransactionSource.Payment, paymentId);

        result.Should().HaveCount(2);
        for (int i = 0; i < result.Count - 1; i++)
        {
            result[i].CreatedAt.Should().BeOnOrBefore(result[i + 1].CreatedAt);
        }
    }

    [Fact]
    public async Task Repository_GetBySource_Should_FilterBySourceType()
    {
        var visitId = Defaults.TariffId;
        await CreateTestTransactionAsync(Defaults.UserId, Defaults.SmallAmount, TransactionType.Withdrawal, TransactionSource.Visit, visitId);
        await CreateTestTransactionAsync(Defaults.UserId, Defaults.DefaultAmount, TransactionType.Deposit, TransactionSource.Manual);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var visitResults = await repository.GetBySourceAsync(TransactionSource.Visit, visitId);
        var manualResults = await repository.GetBySourceAsync(TransactionSource.Manual, Guid.Empty);

        visitResults.Should().HaveCount(1);
        visitResults[0].Source.Should().Be(TransactionSource.Visit);
    }

    [Fact]
    public async Task Repository_GetBySource_Should_FindRefundTransactions()
    {
        var refundId = Defaults.TariffId;
        await CreateTestTransactionAsync(Defaults.UserId, Defaults.SmallAmount, TransactionType.Deposit, TransactionSource.Refund, refundId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var result = await repository.GetBySourceAsync(TransactionSource.Refund, refundId);

        result.Should().HaveCount(1);
        result[0].Source.Should().Be(TransactionSource.Refund);
    }
}
