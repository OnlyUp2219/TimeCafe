namespace Billing.TimeCafe.Test.Integration.Repository.Transaction;

public class ExistsBySourceAsyncTests : BaseTransactionRepositoryTest
{
    [Fact]
    public async Task Repository_ExistsBySource_Should_ReturnTrue_WhenExists()
    {
        var visitId = DefaultsGuid.TariffId;
        await CreateTestTransactionAsync(DefaultsGuid.UserId, DefaultsGuid.SmallAmount, TransactionType.Withdrawal, TransactionSource.Visit, visitId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var exists = await repository.ExistsBySourceAsync(TransactionSource.Visit, visitId);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_ExistsBySource_Should_ReturnFalse_WhenNotExists()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var exists = await repository.ExistsBySourceAsync(TransactionSource.Visit, Guid.NewGuid());

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_ExistsBySource_Should_ReturnFalse_WhenSourceTypeNotMatches()
    {
        var visitId = DefaultsGuid.TariffId;
        await CreateTestTransactionAsync(DefaultsGuid.UserId, DefaultsGuid.SmallAmount, TransactionType.Withdrawal, TransactionSource.Visit, visitId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var exists = await repository.ExistsBySourceAsync(TransactionSource.Payment, visitId);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_ExistsBySource_Should_ReturnTrue_ForDuplicateSourceId()
    {
        var visitId = DefaultsGuid.TariffId;
        await CreateTestTransactionAsync(DefaultsGuid.UserId, DefaultsGuid.SmallAmount, TransactionType.Withdrawal, TransactionSource.Visit, visitId);
        await CreateTestTransactionAsync(DefaultsGuid.UserId2, DefaultsGuid.SmallAmount, TransactionType.Withdrawal, TransactionSource.Visit, visitId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var exists = await repository.ExistsBySourceAsync(TransactionSource.Visit, visitId);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_ExistsBySource_Should_DetectPaymentTransaction()
    {
        var paymentId = Guid.NewGuid();
        await CreateTestTransactionAsync(DefaultsGuid.UserId, DefaultsGuid.DefaultAmount, TransactionType.Deposit, TransactionSource.Payment, paymentId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var exists = await repository.ExistsBySourceAsync(TransactionSource.Payment, paymentId);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_ExistsBySource_Should_DetectRefundTransaction()
    {
        var refundId = Guid.NewGuid();
        await CreateTestTransactionAsync(DefaultsGuid.UserId, DefaultsGuid.SmallAmount, TransactionType.Deposit, TransactionSource.Refund, refundId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var exists = await repository.ExistsBySourceAsync(TransactionSource.Refund, refundId);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_ExistsBySource_Should_ReturnFalse_ForDifferentSourceIds()
    {
        var visitId1 = DefaultsGuid.TariffId;
        var visitId2 = Guid.NewGuid();
        await CreateTestTransactionAsync(DefaultsGuid.UserId, DefaultsGuid.SmallAmount, TransactionType.Withdrawal, TransactionSource.Visit, visitId1);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var exists = await repository.ExistsBySourceAsync(TransactionSource.Visit, visitId2);

        exists.Should().BeFalse();
    }
}
