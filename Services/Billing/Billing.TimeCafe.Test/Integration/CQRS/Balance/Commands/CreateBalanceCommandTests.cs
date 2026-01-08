namespace Billing.TimeCafe.Test.Integration.CQRS.Balance.Commands;

public class CreateBalanceCommandTests : IDisposable
{
    protected HttpClient Client { get; }
    protected IntegrationApiFactory Factory { get; }

    public CreateBalanceCommandTests()
    {
        Factory = new IntegrationApiFactory();
        Client = Factory.CreateClient();
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    [Fact]
    public async Task Command_CreateBalance_Should_CreateNewBalance_WhenNotExists()
    {
        var userId = Defaults.UserId;

        CreateBalanceResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new CreateBalanceCommand(userId));
        }

        result.Success.Should().BeTrue();
        result.Balance.Should().NotBeNull();
        result.Balance!.UserId.Should().Be(userId);
        result.Balance.CurrentBalance.Should().Be(0m);
        result.Balance.TotalDeposited.Should().Be(0m);
        result.Balance.Debt.Should().Be(0m);

        using var scope2 = CreateScope();
        var db = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var created = await db.Balances.FindAsync(userId);
        created.Should().NotBeNull();
        created!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Command_CreateBalance_Should_ReturnError_WhenAlreadyExists()
    {
        var userId = Defaults.UserId2;

        using (var scope = CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            await repo.CreateAsync(new BalanceModel(userId));
        }

        CreateBalanceResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new CreateBalanceCommand(userId));
        }

        result.Success.Should().BeFalse();
        result.Code.Should().Be("BalanceAlreadyExists");
        result.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Command_CreateBalance_Should_ThrowValidationException_WhenUserIdEmpty()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new CreateBalanceCommand(InvalidData.EmptyUserId));
        await action.Should().ThrowAsync<ValidationException>();
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
