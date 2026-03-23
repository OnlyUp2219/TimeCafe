namespace Billing.TimeCafe.Test.Integration.CQRS.Balance.Queries;

public class GetBalanceQueryTests : IDisposable
{
    protected HttpClient Client { get; }
    protected IntegrationApiFactory Factory { get; }

    public GetBalanceQueryTests()
    {
        Factory = new IntegrationApiFactory();
        Client = Factory.CreateClient();
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    [Fact]
    public async Task Query_GetBalance_Should_CreateBalance_WhenNotExists()
    {
        var userId = DefaultsGuid.UserId3;

        using (var scope = CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var before = await db.Balances.FindAsync(userId);
            before.Should().BeNull();
        }

        GetBalanceResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetBalanceQuery(userId));
        }

        result.Success.Should().BeTrue();
        result.Balance.Should().NotBeNull();
        result.Balance!.UserId.Should().Be(userId);
        result.Balance.CurrentBalance.Should().Be(0m);

        using var scope2 = CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var created = await db2.Balances.FindAsync(userId);
        created.Should().NotBeNull();
    }

    [Fact]
    public async Task Query_GetBalance_Should_ReturnExistingBalance_WhenExists()
    {
        var userId = DefaultsGuid.UserId;

        using (var scope = CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            var balance = new BalanceModel(userId) { CurrentBalance = DefaultsGuid.UpdatedAmount, TotalDeposited = DefaultsGuid.UpdatedAmount };
            await repo.CreateAsync(balance);
        }

        GetBalanceResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetBalanceQuery(userId));
        }

        result.Success.Should().BeTrue();
        result.Balance.Should().NotBeNull();
        result.Balance!.CurrentBalance.Should().Be(DefaultsGuid.UpdatedAmount);
        result.Balance.TotalDeposited.Should().Be(DefaultsGuid.UpdatedAmount);
    }

    [Fact]
    public async Task Query_GetBalance_Should_ThrowValidationException_WhenUserIdEmpty()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetBalanceQuery(InvalidDataGuid.EmptyUserId));
        await action.Should().ThrowAsync<ValidationException>();
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
