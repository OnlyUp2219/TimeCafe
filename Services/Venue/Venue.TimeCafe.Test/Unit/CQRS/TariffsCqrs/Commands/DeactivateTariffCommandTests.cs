namespace Venue.TimeCafe.Test.Unit.CQRS.TariffsCqrs.Commands;

public class DeactivateTariffCommandTests : BaseCqrsHandlerTest
{
    private readonly DeactivateTariffCommandHandler _handler;

    public DeactivateTariffCommandTests()
    {
        _handler = new DeactivateTariffCommandHandler(TariffRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenTariffDeactivated()
    {
        var command = new DeactivateTariffCommand(1);
        var tariff = new Tariff { TariffId = 1, Name = "Test", PricePerMinute = 10m, BillingType = BillingType.PerMinute };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.DeactivateAsync(1)).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenTariffDoesNotExist()
    {
        var command = new DeactivateTariffCommand(999);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Tariff?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("TariffNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsFalse()
    {
        var command = new DeactivateTariffCommand(1);
        var tariff = new Tariff { TariffId = 1, Name = "Test", PricePerMinute = 10m, BillingType = BillingType.PerMinute };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.DeactivateAsync(1)).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeactivateTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var command = new DeactivateTariffCommand(1);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeactivateTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(0, false, "ID тарифа обязателен")]
    [InlineData(-1, false, "ID тарифа обязателен")]
    [InlineData(1, true, null)]
    [InlineData(999, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int tariffId, bool isValid, string? expectedError)
    {
        var command = new DeactivateTariffCommand(tariffId);
        var validator = new DeactivateTariffCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
