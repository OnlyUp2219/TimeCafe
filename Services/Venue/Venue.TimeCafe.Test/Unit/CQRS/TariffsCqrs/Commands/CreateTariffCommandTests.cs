namespace Venue.TimeCafe.Test.Unit.CQRS.TariffsCqrs.Commands;

public class CreateTariffCommandTests : BaseCqrsHandlerTest
{
    private readonly CreateTariffCommandHandler _handler;

    public CreateTariffCommandTests()
    {
        _handler = new CreateTariffCommandHandler(TariffRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenTariffCreated()
    {
        var command = new CreateTariffCommand("Test Tariff", "Description", 10.5m, BillingType.PerMinute, null, true);
        var tariff = new Tariff
        {
            TariffId = 1,
            Name = "Test Tariff",
            Description = "Description",
            PricePerMinute = 10.5m,
            BillingType = BillingType.PerMinute,
            IsActive = true
        };

        TariffRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Tariff>())).ReturnsAsync(tariff);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tariff.Should().NotBeNull();
        result.Tariff!.Name.Should().Be("Test Tariff");
        result.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryFails()
    {
        var command = new CreateTariffCommand("Test Tariff", "Description", 10.5m, BillingType.PerMinute, null, true);

        TariffRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Tariff>())).ThrowsAsync(new InvalidOperationException("Database error"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreateTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var command = new CreateTariffCommand("Test Tariff", "Description", 10.5m, BillingType.PerMinute, null, true);

        TariffRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Tariff>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreateTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", "Desc", 10, BillingType.PerMinute, false, "Название тарифа обязательно")]
    [InlineData(null, "Desc", 10, BillingType.PerMinute, false, "Название тарифа обязательно")]
    [InlineData("Valid Name", "Desc", 0, BillingType.PerMinute, false, "Цена за минуту должна быть больше 0")]
    [InlineData("Valid Name", "Desc", -1, BillingType.PerMinute, false, "Цена за минуту должна быть больше 0")]
    [InlineData("Valid Name", "Desc", 10, BillingType.PerMinute, true, null)]
    public async Task Validator_Should_ValidateCorrectly(string? name, string? description, decimal price, BillingType billingType, bool isValid, string? expectedError)
    {
        var command = new CreateTariffCommand(name!, description, price, billingType, null, true);
        var validator = new CreateTariffCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
