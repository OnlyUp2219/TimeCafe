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
        var userId = DefaultsGuid.UserId;

        Result<CreateBalanceResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new CreateBalanceCommand(userId));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserId.Should().Be(userId);
        result.Value.CurrentBalance.Should().Be(0m);

        using var scope2 = CreateScope();
        var db = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var created = await db.Balances.FindAsync(userId);
        created.Should().NotBeNull();
        created!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Command_CreateBalance_Should_ReturnExistingBalance_WhenAlreadyExists()
    {
        var userId = DefaultsGuid.UserId2;

        using (var scope = CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
            await repo.CreateAsync(new BalanceModel(userId) { CurrentBalance = 100m });
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.SaveChangesAsync();
        }

        Result<CreateBalanceResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new CreateBalanceCommand(userId));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.CurrentBalance.Should().Be(100m);
    }

    [Fact]
    public async Task Command_CreateBalance_Should_ThrowValidationException_WhenUserIdEmpty()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new CreateBalanceCommand(InvalidDataGuid.EmptyUserId));
        await action.Should().ThrowAsync<ValidationException>();
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
