using Venue.TimeCafe.Domain.DTOs;
using Venue.TimeCafe.Test.Integration.Helpers;

namespace Venue.TimeCafe.Test.Unit.CQRS.TariffsCqrs.Commands;

public class UpdateTariffCommandTests : BaseCqrsHandlerTest
{
    private readonly UpdateTariffCommandHandler _handler;

    public UpdateTariffCommandTests()
    {
        _handler = new UpdateTariffCommandHandler(TariffRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenTariffUpdated()
    {
        var tariffId = Guid.NewGuid();
        var tariffDto = new TariffWithThemeDto
        {
            TariffId = tariffId,
            TariffName = "Updated Tariff",
            TariffDescription = "Updated",
            TariffPricePerMinute = TestData.ExistingTariffs.Tariff2PricePerMinute,
            TariffBillingType = BillingType.Hourly,
            TariffIsActive = true
        };
        var tariff = new Tariff
        {
            TariffId = tariffId,
            Name = "Updated Tariff",
            Description = "Updated",
            PricePerMinute = TestData.ExistingTariffs.Tariff2PricePerMinute,
            BillingType = BillingType.Hourly,
            IsActive = true
        };
        var command = new UpdateTariffCommand(
            tariffId.ToString(),
            "Updated Tariff",
            "Updated",
            TestData.ExistingTariffs.Tariff2PricePerMinute,
            BillingType.Hourly,
            null,
            true);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ReturnsAsync(tariffDto);
        TariffRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Tariff>())).ReturnsAsync(tariff);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tariff.Should().NotBeNull();
        result.Tariff!.Name.Should().Be("Updated Tariff");
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenTariffDoesNotExist()
    {
        var tariffId = TestData.NonExistingIds.NonExistingTariffId;
        var command = new UpdateTariffCommand(
            tariffId.ToString(),
            "Nonexistent",
            "Desc",
            TestData.ExistingTariffs.Tariff1PricePerMinute,
            BillingType.PerMinute,
            null,
            true);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ReturnsAsync((TariffWithThemeDto?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("TariffNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsNull()
    {
        var tariffId = Guid.NewGuid();
        var tariffDto = new TariffWithThemeDto
        {
            TariffId = tariffId,
            TariffName = TestData.ExistingTariffs.Tariff1Name,
            TariffPricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            TariffBillingType = BillingType.PerMinute
        };
        var command = new UpdateTariffCommand(
            tariffId.ToString(),
            TestData.ExistingTariffs.Tariff1Name,
            "Desc",
            TestData.ExistingTariffs.Tariff1PricePerMinute,
            BillingType.PerMinute,
            null,
            true);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ReturnsAsync(tariffDto);
        TariffRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Tariff>())).ThrowsAsync(new Exception("Update failed"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("UpdateTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var tariffId = Guid.NewGuid();
        var command = new UpdateTariffCommand(
            tariffId.ToString(),
            TestData.ExistingTariffs.Tariff1Name,
            "Desc",
            TestData.ExistingTariffs.Tariff1PricePerMinute,
            BillingType.PerMinute,
            null,
            true);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("UpdateTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", "Name", "Desc", 10, false, "Тариф не найден")]
    [InlineData("not-a-guid", "Name", "Desc", 10, false, "Тариф не найден")]
    [InlineData("00000000-0000-0000-0000-000000000000", "Name", "Desc", 10, false, "Тариф не найден")]
    public async Task Validator_Should_ValidateCorrectly_InvalidTariffId(string tariffId, string? name, string? description, decimal price, bool isValid, string? expectedError)
    {
        var command = new UpdateTariffCommand(tariffId, name!, description!, price, BillingType.PerMinute, null, true);
        var validator = new UpdateTariffCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }

    [Theory]
    [InlineData("", "Desc", 10, false, "Название тарифа обязательно")]
    [InlineData(null, "Desc", 10, false, "Название тарифа обязательно")]
    [InlineData("Name", "Desc", 0, false, "Цена за минуту должна быть больше 0")]
    [InlineData("Name", "Desc", -1, false, "Цена за минуту должна быть больше 0")]
    [InlineData("Name", "Desc", 10, true, null)]
    public async Task Validator_Should_ValidateCorrectly_FieldValidation(string? name, string? description, decimal price, bool isValid, string? expectedError)
    {
        var command = new UpdateTariffCommand(Guid.NewGuid().ToString(), name!, description!, price, BillingType.PerMinute, null, true);
        var validator = new UpdateTariffCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
