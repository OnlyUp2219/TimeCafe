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
        var userId = Defaults.UserId;

        GetTransactionHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetTransactionHistoryQuery(userId, Page: 1, PageSize: 10));
        }

        result.Success.Should().BeTrue();
        result.Transactions.Should().NotBeNull();
        result.Transactions.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Query_GetTransactionHistory_Should_ReturnTransactions_WhenExists()
    {
        var userId = Defaults.UserId;

        using (var scope = CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

            var transaction1 = TransactionModel.CreateDeposit(
                userId,
                Defaults.DefaultAmount,
                TransactionSource.Payment,
                Defaults.PaymentId,
                comment: "First transaction");
            transaction1.BalanceAfter = Defaults.DefaultAmount;

            var transaction2 = TransactionModel.CreateWithdrawal(
                userId,
                Defaults.SmallAmount,
                TransactionSource.Visit,
                Defaults.TariffId,
                comment: "Second transaction");
            transaction2.BalanceAfter = Defaults.DefaultAmount - Defaults.SmallAmount;

            await repo.CreateAsync(transaction1);
            await repo.CreateAsync(transaction2);
        }

        GetTransactionHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetTransactionHistoryQuery(userId, Page: 1, PageSize: 10));
        }

        result.Success.Should().BeTrue();
        result.Transactions.Should().NotBeNull();
        result.Transactions.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(1);
        result.Transactions![0].UserId.Should().Be(userId);
        result.Transactions[1].UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Query_GetTransactionHistory_Should_RespectPagination()
    {
        var userId = Defaults.UserId2;

        using (var scope = CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

            for (int i = 0; i < 5; i++)
            {
                var transaction = TransactionModel.CreateDeposit(
                    userId,
                    Defaults.SmallAmount * (i + 1),
                    TransactionSource.Payment,
                    Guid.NewGuid(),
                    comment: $"Transaction {i + 1}");
                transaction.BalanceAfter = Defaults.SmallAmount * (i + 1);

                await repo.CreateAsync(transaction);
            }
        }

        GetTransactionHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetTransactionHistoryQuery(userId, Page: 1, PageSize: 2));
        }

        result.Success.Should().BeTrue();
        result.Transactions.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Query_GetTransactionHistory_Should_ReturnSecondPage()
    {
        var userId = Defaults.UserId3;

        using (var scope = CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

            for (int i = 0; i < 5; i++)
            {
                var transaction = TransactionModel.CreateDeposit(
                    userId,
                    Defaults.SmallAmount,
                    TransactionSource.Payment,
                    Guid.NewGuid(),
                    comment: $"Transaction {i + 1}");
                transaction.BalanceAfter = Defaults.SmallAmount * (i + 1);

                await repo.CreateAsync(transaction);
            }
        }

        GetTransactionHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetTransactionHistoryQuery(userId, Page: 2, PageSize: 2));
        }

        result.Success.Should().BeTrue();
        result.Transactions.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Query_GetTransactionHistory_Should_ThrowValidationException_WhenUserIdEmpty()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetTransactionHistoryQuery(InvalidData.EmptyUserId, Page: 1, PageSize: 10));
        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Query_GetTransactionHistory_Should_ThrowValidationException_WhenPageZero()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetTransactionHistoryQuery(Defaults.UserId, Page: 0, PageSize: 10));
        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Query_GetTransactionHistory_Should_ThrowValidationException_WhenPageSizeZero()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetTransactionHistoryQuery(Defaults.UserId, Page: 1, PageSize: 0));
        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Query_GetTransactionHistory_Should_ThrowValidationException_WhenPageSizeGreaterThan100()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetTransactionHistoryQuery(Defaults.UserId, Page: 1, PageSize: 101));
        await action.Should().ThrowAsync<ValidationException>();
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
