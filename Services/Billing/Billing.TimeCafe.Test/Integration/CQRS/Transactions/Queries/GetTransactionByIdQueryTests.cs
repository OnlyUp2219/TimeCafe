using FluentResults;

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
        var transactionId = DefaultsGuid.TransactionId;
        var userId = DefaultsGuid.UserId;

        using (var scope = CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
            var transaction = TransactionModel.CreateDeposit(
                userId,
                DefaultsGuid.DefaultAmount,
                TransactionSource.Payment,
                DefaultsGuid.PaymentId,
                comment: "Test transaction");
            transaction.TransactionId = transactionId;
            transaction.BalanceAfter = DefaultsGuid.DefaultAmount;
            await repo.CreateAsync(transaction);
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.SaveChangesAsync();
        }

        Result<GetTransactionByIdResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetTransactionByIdQuery(transactionId));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Transaction.TransactionId.Should().Be(transactionId);
        result.Value.Transaction.UserId.Should().Be(userId);
        result.Value.Transaction.Amount.Should().Be(DefaultsGuid.DefaultAmount);
        result.Value.Transaction.Type.Should().Be((int)TransactionType.Deposit);
        result.Value.Transaction.Source.Should().Be((int)TransactionSource.Payment);
        result.Value.Transaction.Status.Should().Be((int)TransactionStatus.Completed);
        result.Value.Transaction.Comment.Should().Be("Test transaction");
        result.Value.Transaction.BalanceAfter.Should().Be(DefaultsGuid.DefaultAmount);
    }

    [Fact]
    public async Task Query_GetTransactionById_Should_ReturnNotFound_WhenNotExists()
    {
        var nonExistentId = InvalidDataGuid.NonExistentPaymentId;

        Result<GetTransactionByIdResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetTransactionByIdQuery(nonExistentId));
        }

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("не найдена"));
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
