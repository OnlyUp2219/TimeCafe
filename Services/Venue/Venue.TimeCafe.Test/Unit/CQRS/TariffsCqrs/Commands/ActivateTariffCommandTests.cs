namespace Venue.TimeCafe.Test.Unit.CQRS.TariffsCqrs.Commands;

public class ActivateTariffCommandTests : BaseCqrsHandlerTest
{
    private readonly ActivateTariffCommandHandler _handler;

    public ActivateTariffCommandTests()
    {
        _handler = new ActivateTariffCommandHandler(TariffRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenTariffActivated()
    {
        var command = new ActivateTariffCommand(1);
        var tariff = new Tariff { TariffId = 1, Name = "Test", PricePerMinute = 10m, BillingType = BillingType.PerMinute };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.ActivateAsync(1)).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenTariffDoesNotExist()
    {
        var command = new ActivateTariffCommand(999);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Tariff?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("TariffNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsFalse()
    {
        var command = new ActivateTariffCommand(1);
        var tariff = new Tariff { TariffId = 1, Name = "Test", PricePerMinute = 10m, BillingType = BillingType.PerMinute };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.ActivateAsync(1)).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("ActivateTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var command = new ActivateTariffCommand(1);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("ActivateTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(0, false, "ID тарифа обязателен")]
    [InlineData(-1, false, "ID тарифа обязателен")]
    [InlineData(1, true, null)]
    [InlineData(999, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int tariffId, bool isValid, string? expectedError)
    {
        var command = new ActivateTariffCommand(tariffId);
        var validator = new ActivateTariffCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
