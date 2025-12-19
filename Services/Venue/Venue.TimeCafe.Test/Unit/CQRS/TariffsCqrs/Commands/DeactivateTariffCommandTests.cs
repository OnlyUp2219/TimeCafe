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
        var tariffId = Guid.NewGuid();
        var command = new DeactivateTariffCommand(tariffId.ToString());
        var tariff = new TariffWithThemeDto
        {
            TariffId = tariffId,
            Name = TestData.ExistingTariffs.Tariff1Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            BillingType = TestData.ExistingTariffs.Tariff1BillingType
        };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.DeactivateAsync(tariffId)).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenTariffDoesNotExist()
    {
        var tariffId = TestData.NonExistingIds.NonExistingTariffId;
        var command = new DeactivateTariffCommand(tariffId.ToString());

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ReturnsAsync((TariffWithThemeDto?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("TariffNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsFalse()
    {
        var tariffId = Guid.NewGuid();
        var command = new DeactivateTariffCommand(tariffId.ToString());
        var tariff = new TariffWithThemeDto
        {
            TariffId = tariffId,
            Name = TestData.ExistingTariffs.Tariff1Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            BillingType = TestData.ExistingTariffs.Tariff1BillingType
        };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.DeactivateAsync(tariffId)).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeactivateTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var tariffId = Guid.NewGuid();
        var command = new DeactivateTariffCommand(tariffId.ToString());

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeactivateTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false, "Тариф не найден")]
    [InlineData("not-a-guid", false, "Тариф не найден")]
    [InlineData("00000000-0000-0000-0000-000000000000", false, "Тариф не найден")]
    public async Task Validator_Should_ValidateCorrectly_InvalidCases(string tariffId, bool isValid, string? expectedError)
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

    [Fact]
    public async Task Validator_Should_ValidateCorrectly_ValidGuid()
    {
        var command = new DeactivateTariffCommand(Guid.NewGuid().ToString());
        var validator = new DeactivateTariffCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }
}
