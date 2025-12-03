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
        var tariff = new Tariff
        {
            TariffId = 1,
            Name = "Updated Tariff",
            Description = "Updated",
            PricePerMinute = 15m,
            BillingType = BillingType.Hourly,
            IsActive = true
        };
        var command = new UpdateTariffCommand(tariff);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.UpdateAsync(tariff)).ReturnsAsync(tariff);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Tariff.Should().NotBeNull();
        result.Tariff!.Name.Should().Be("Updated Tariff");
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenTariffDoesNotExist()
    {
        var tariff = new Tariff
        {
            TariffId = 999,
            Name = "Nonexistent",
            PricePerMinute = 10m,
            BillingType = BillingType.PerMinute
        };
        var command = new UpdateTariffCommand(tariff);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Tariff?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("TariffNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsNull()
    {
        var tariff = new Tariff
        {
            TariffId = 1,
            Name = "Test",
            PricePerMinute = 10m,
            BillingType = BillingType.PerMinute
        };
        var command = new UpdateTariffCommand(tariff);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tariff);
        TariffRepositoryMock.Setup(r => r.UpdateAsync(tariff)).ThrowsAsync(new Exception("Update failed"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("UpdateTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var tariff = new Tariff
        {
            TariffId = 1,
            Name = "Test",
            PricePerMinute = 10m,
            BillingType = BillingType.PerMinute
        };
        var command = new UpdateTariffCommand(tariff);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("UpdateTariffFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(null, "Name", "Desc", 10, false, "Тариф обязателен")]
    [InlineData(0, "Name", "Desc", 10, false, "ID тарифа обязателен")]
    [InlineData(-1, "Name", "Desc", 10, false, "ID тарифа обязателен")]
    [InlineData(1, "", "Desc", 10, false, "Название тарифа обязательно")]
    [InlineData(1, null, "Desc", 10, false, "Название тарифа обязательно")]
    [InlineData(1, "Name", "Desc", 0, false, "Цена за минуту должна быть больше 0")]
    [InlineData(1, "Name", "Desc", -1, false, "Цена за минуту должна быть больше 0")]
    [InlineData(1, "Name", "Desc", 10, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int? tariffId, string? name, string? description, decimal price, bool isValid, string? expectedError)
    {
        var tariff = tariffId.HasValue ? new Tariff
        {
            TariffId = tariffId.Value,
            Name = name!,
            Description = description,
            PricePerMinute = price,
            BillingType = BillingType.PerMinute
        } : null;
        var command = new UpdateTariffCommand(tariff!);
        var validator = new UpdateTariffCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
