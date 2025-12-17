using Venue.TimeCafe.Domain.DTOs;
using Venue.TimeCafe.Test.Integration.Helpers;

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
        var command = new DeleteTariffCommand(tariffId.ToString());
        var tariff = new TariffWithThemeDto
        {
            TariffId = tariffId,
            TariffName = TestData.ExistingTariffs.Tariff1Name,
            TariffPricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            TariffBillingType = TestData.ExistingTariffs.Tariff1BillingType
        };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.DeleteAsync(tariffId)).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenTariffDoesNotExist()
    {
        var tariffId = TestData.NonExistingIds.NonExistingTariffId;
        var command = new DeleteTariffCommand(tariffId.ToString());

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
        var command = new DeleteTariffCommand(tariffId.ToString());
        var tariff = new TariffWithThemeDto
        {
            TariffId = tariffId,
            TariffName = TestData.ExistingTariffs.Tariff1Name,
            TariffPricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            TariffBillingType = TestData.ExistingTariffs.Tariff1BillingType
        };

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.DeleteAsync(tariffId)).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeleteTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var tariffId = Guid.NewGuid();
        var command = new DeleteTariffCommand(tariffId.ToString());

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeleteTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false, "Тариф не найден")]
    [InlineData("not-a-guid", false, "Тариф не найден")]
    [InlineData("00000000-0000-0000-0000-000000000000", false, "Тариф не найден")]
    public async Task Validator_Should_ValidateCorrectly_InvalidCases(string tariffId, bool isValid, string? expectedError)
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

    [Fact]
    public async Task Validator_Should_ValidateCorrectly_ValidGuid()
    {
        var command = new DeleteTariffCommand(Guid.NewGuid().ToString());
        var validator = new DeleteTariffCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }
}
