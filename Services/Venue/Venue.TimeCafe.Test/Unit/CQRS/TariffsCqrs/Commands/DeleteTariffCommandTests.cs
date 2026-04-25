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
        var tariffId = Guid.NewGuid();
        var command = new DeleteTariffCommand(tariffId);
        var tariff = new TariffWithThemeDto
        {
            TariffId = tariffId,
            Name = TestData.ExistingTariffs.Tariff1Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            BillingType = TestData.ExistingTariffs.Tariff1BillingType
        };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId, It.IsAny<CancellationToken>())).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.DeleteAsync(tariffId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenTariffDoesNotExist()
    {
        var tariffId = TestData.NonExistingIds.NonExistingTariffId;
        var command = new DeleteTariffCommand(tariffId);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId, It.IsAny<CancellationToken>())).ReturnsAsync((TariffWithThemeDto?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsFalse()
    {
        var tariffId = Guid.NewGuid();
        var command = new DeleteTariffCommand(tariffId);
        var tariff = new TariffWithThemeDto
        {
            TariffId = tariffId,
            Name = TestData.ExistingTariffs.Tariff1Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            BillingType = TestData.ExistingTariffs.Tariff1BillingType
        };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId, It.IsAny<CancellationToken>())).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.DeleteAsync(tariffId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var tariffId = Guid.NewGuid();
        var command = new DeleteTariffCommand(tariffId);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", false, "Тариф не найден")]
    public async Task Validator_Should_ValidateCorrectly_InvalidCases(string tariffIdStr, bool isValid, string? expectedError)
    {
        var command = new DeleteTariffCommand(Guid.Parse(tariffIdStr));
        var validator = new DeleteTariffCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }

    [Fact]
    public async Task Validator_Should_ValidateCorrectly_ValidGuid()
    {
        var command = new DeleteTariffCommand(Guid.NewGuid());
        var validator = new DeleteTariffCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }
}

