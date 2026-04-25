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
        var tariffId = Guid.NewGuid();
        var command = new ActivateTariffCommand(tariffId);
        var tariff = new TariffWithThemeDto
        {
            TariffId = tariffId,
            Name = TestData.ExistingTariffs.Tariff1Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            BillingType = TestData.ExistingTariffs.Tariff1BillingType
        };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId, It.IsAny<CancellationToken>())).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.ActivateAsync(tariffId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenTariffDoesNotExist()
    {
        var tariffId = TestData.NonExistingIds.NonExistingTariffId;
        var command = new ActivateTariffCommand(tariffId);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId, It.IsAny<CancellationToken>())).ReturnsAsync((TariffWithThemeDto?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsFalse()
    {
        var tariffId = Guid.NewGuid();
        var command = new ActivateTariffCommand(tariffId);
        var tariff = new TariffWithThemeDto
        {
            TariffId = tariffId,
            Name = TestData.ExistingTariffs.Tariff1Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            BillingType = TestData.ExistingTariffs.Tariff1BillingType
        };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId, It.IsAny<CancellationToken>())).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.ActivateAsync(tariffId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var tariffId = Guid.NewGuid();
        var command = new ActivateTariffCommand(tariffId);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", true)]
    public async Task Validator_Should_ValidateCorrectly(string tariffIdStr, bool isValid)
    {
        var command = new ActivateTariffCommand(Guid.Parse(tariffIdStr));
        var validator = new ActivateTariffCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
    }
}

