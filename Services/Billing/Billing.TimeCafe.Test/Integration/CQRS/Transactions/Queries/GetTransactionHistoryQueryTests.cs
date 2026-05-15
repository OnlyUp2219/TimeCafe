using FluentResults;

namespace Billing.TimeCafe.Test.Integration.CQRS.Transactions.Queries;

public class GetTransactionHistoryQueryTests : IDisposable
{
    protected HttpClient Client { get; }
    protected IntegrationApiFactory Factory { get; }

    public GetTransactionHistoryQueryTests()
    {
        Factory = new IntegrationApiFactory();
        Client = Factory.CreateClient();
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    [Fact]
    public async Task Query_GetTransactionHistory_Should_ReturnEmpty_WhenNoTransactions()
    {
        var userId = DefaultsGuid.UserId;

        Result<GetTransactionHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetTransactionHistoryQuery(userId, Page: 1, PageSize: 10));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.Transactions.Should().NotBeNull();
        result.Value.Transactions.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Query_GetTransactionHistory_Should_ReturnTransactions_WhenExists()
    {
        var userId = DefaultsGuid.UserId;
        var userIdGuid = userId;

        using (var scope = CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

            var transaction1 = TransactionModel.CreateDeposit(
                userIdGuid,
                DefaultsGuid.DefaultAmount,
                TransactionSource.Payment,
                DefaultsGuid.PaymentId,
                comment: "First transaction");
            transaction1.BalanceAfter = DefaultsGuid.DefaultAmount;

            var transaction2 = TransactionModel.CreateWithdrawal(
                userIdGuid,
                DefaultsGuid.SmallAmount,
                TransactionSource.Visit,
                DefaultsGuid.TariffId,
                comment: "Second transaction");
            transaction2.BalanceAfter = DefaultsGuid.DefaultAmount - DefaultsGuid.SmallAmount;

            await repo.CreateAsync(transaction1);
            await repo.CreateAsync(transaction2);
        }

        Result<GetTransactionHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetTransactionHistoryQuery(userId, Page: 1, PageSize: 10));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.Transactions.Should().NotBeNull();
        result.Value.Transactions.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.TotalPages.Should().Be(1);
        result.Value.Transactions[0].UserId.Should().Be(userId);
        result.Value.Transactions[1].UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Query_GetTransactionHistory_Should_RespectPagination()
    {
        var userId = DefaultsGuid.UserId2;
        var userIdGuid = userId;
        var sourceIds = new[] { DefaultsGuid.PaymentId, DefaultsGuid.PaymentId2, DefaultsGuid.PaymentId3, DefaultsGuid.PaymentId4, DefaultsGuid.PaymentId5 };

        using (var scope = CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

            for (int i = 0; i < 5; i++)
            {
                var transaction = TransactionModel.CreateDeposit(
                    userIdGuid,
                    DefaultsGuid.SmallAmount * (i + 1),
                    TransactionSource.Payment,
                    sourceIds[i % sourceIds.Length],
                    comment: $"Transaction {i + 1}");
                transaction.BalanceAfter = DefaultsGuid.SmallAmount * (i + 1);

                await repo.CreateAsync(transaction);
            }
        }

        Result<GetTransactionHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetTransactionHistoryQuery(userId, Page: 1, PageSize: 2));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.Transactions.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(5);
        result.Value.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Query_GetTransactionHistory_Should_ReturnSecondPage()
    {
        var userId = DefaultsGuid.UserId3;
        var userIdGuid = userId;
        var sourceIds = new[] { DefaultsGuid.PaymentId, DefaultsGuid.PaymentId2, DefaultsGuid.PaymentId3, DefaultsGuid.PaymentId4, DefaultsGuid.PaymentId5 };

        using (var scope = CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

            for (int i = 0; i < 5; i++)
            {
                var transaction = TransactionModel.CreateDeposit(
                    userIdGuid,
                    DefaultsGuid.SmallAmount,
                    TransactionSource.Payment,
                    sourceIds[i % sourceIds.Length],
                    comment: $"Transaction {i + 1}");
                transaction.BalanceAfter = DefaultsGuid.SmallAmount * (i + 1);

                await repo.CreateAsync(transaction);
            }
        }

        Result<GetTransactionHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetTransactionHistoryQuery(userId, Page: 2, PageSize: 2));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.Transactions.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(5);
        result.Value.TotalPages.Should().Be(3);
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
