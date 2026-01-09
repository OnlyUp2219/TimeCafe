namespace Billing.TimeCafe.Test.Integration.CQRS.Transactions.Queries;

public class GetTransactionByIdQueryTests : IDisposable
{
    protected HttpClient Client { get; }
    protected IntegrationApiFactory Factory { get; }

    public GetTransactionByIdQueryTests()
    {
        Factory = new IntegrationApiFactory();
        Client = Factory.CreateClient();
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    [Fact]
    public async Task Query_GetTransactionById_Should_ReturnTransaction_WhenExists()
    {
        var transactionId = Defaults.TransactionId;
        var userId = Defaults.UserId;

        using (var scope = CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
            var transaction = TransactionModel.CreateDeposit(
                userId,
                Defaults.DefaultAmount,
                TransactionSource.Payment,
                Defaults.PaymentId,
                comment: "Test transaction");
            transaction.TransactionId = transactionId;
            transaction.BalanceAfter = Defaults.DefaultAmount;
            await repo.CreateAsync(transaction);
        }

        GetTransactionByIdResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetTransactionByIdQuery(transactionId));
        }

        result.Success.Should().BeTrue();
        result.Transaction.Should().NotBeNull();
        result.Transaction!.TransactionId.Should().Be(transactionId);
        result.Transaction.UserId.Should().Be(userId);
        result.Transaction.Amount.Should().Be(Defaults.DefaultAmount);
        result.Transaction.Type.Should().Be((int)TransactionType.Deposit);
        result.Transaction.Source.Should().Be((int)TransactionSource.Payment);
        result.Transaction.SourceId.Should().Be(Defaults.PaymentId);
        result.Transaction.Status.Should().Be((int)TransactionStatus.Completed);
        result.Transaction.Comment.Should().Be("Test transaction");
        result.Transaction.BalanceAfter.Should().Be(Defaults.DefaultAmount);
    }

    [Fact]
    public async Task Query_GetTransactionById_Should_ReturnNotFound_WhenNotExists()
    {
        var nonExistentId = InvalidData.NonExistentPaymentId;

        GetTransactionByIdResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetTransactionByIdQuery(nonExistentId));
        }

        result.Success.Should().BeFalse();
        result.Code.Should().Be("TransactionNotFound");
        result.StatusCode.Should().Be(404);
        result.Transaction.Should().BeNull();
    }

    [Fact]
    public async Task Query_GetTransactionById_Should_ThrowValidationException_WhenTransactionIdEmpty()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetTransactionByIdQuery(InvalidData.EmptyUserId));
        await action.Should().ThrowAsync<ValidationException>();
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
