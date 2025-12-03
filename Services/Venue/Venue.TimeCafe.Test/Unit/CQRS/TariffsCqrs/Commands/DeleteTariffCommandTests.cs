namespace Venue.TimeCafe.Test.Unit.CQRS.TariffsCqrs.Commands;

public class DeleteTariffCommandTests : BaseCqrsHandlerTest
{
    private readonly DeleteTariffCommandHandler _handler;

    public DeleteTariffCommandTests()
    {
        _handler = new DeleteTariffCommandHandler(TariffRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenTariffDeleted()
    {
        var command = new DeleteTariffCommand(1);
        var tariff = new Tariff { TariffId = 1, Name = "Test", PricePerMinute = 10m, BillingType = BillingType.PerMinute };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenTariffDoesNotExist()
    {
        var command = new DeleteTariffCommand(999);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Tariff?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("TariffNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsFalse()
    {
        var command = new DeleteTariffCommand(1);
        var tariff = new Tariff { TariffId = 1, Name = "Test", PricePerMinute = 10m, BillingType = BillingType.PerMinute };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeleteTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var command = new DeleteTariffCommand(1);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeleteTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(0, false, "ID тарифа обязателен")]
    [InlineData(-1, false, "ID тарифа обязателен")]
    [InlineData(1, true, null)]
    [InlineData(999, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int tariffId, bool isValid, string? expectedError)
    {
        var command = new DeleteTariffCommand(tariffId);
        var validator = new DeleteTariffCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
