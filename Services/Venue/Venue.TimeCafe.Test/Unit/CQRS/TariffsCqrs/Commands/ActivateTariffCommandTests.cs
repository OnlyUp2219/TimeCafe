using Venue.TimeCafe.Domain.DTOs;
using Venue.TimeCafe.Test.Integration.Helpers;

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
        var command = new ActivateTariffCommand(tariffId.ToString());
        var tariff = new TariffWithThemeDto
        {
            TariffId = tariffId,
            TariffName = TestData.ExistingTariffs.Tariff1Name,
            TariffPricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            TariffBillingType = TestData.ExistingTariffs.Tariff1BillingType
        };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.ActivateAsync(tariffId)).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenTariffDoesNotExist()
    {
        var tariffId = TestData.NonExistingIds.NonExistingTariffId;
        var command = new ActivateTariffCommand(tariffId.ToString());

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
        var command = new ActivateTariffCommand(tariffId.ToString());
        var tariff = new TariffWithThemeDto
        {
            TariffId = tariffId,
            TariffName = TestData.ExistingTariffs.Tariff1Name,
            TariffPricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            TariffBillingType = TestData.ExistingTariffs.Tariff1BillingType
        };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.ActivateAsync(tariffId)).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("ActivateTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var tariffId = Guid.NewGuid();
        var command = new ActivateTariffCommand(tariffId.ToString());

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("ActivateTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("not-a-guid", false)]
    [InlineData("00000000-0000-0000-0000-000000000000", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", true)]
    public async Task Validator_Should_ValidateCorrectly(string tariffId, bool isValid)
    {
        var command = new ActivateTariffCommand(tariffId);
        var validator = new ActivateTariffCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
    }
}
