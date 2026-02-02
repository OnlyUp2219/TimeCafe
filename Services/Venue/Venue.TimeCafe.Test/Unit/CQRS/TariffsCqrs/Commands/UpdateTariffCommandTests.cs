namespace Venue.TimeCafe.Test.Unit.CQRS.TariffsCqrs.Commands;

public class UpdateTariffCommandTests : BaseCqrsHandlerTest
{
    private readonly UpdateTariffCommandHandler _handler;

    public UpdateTariffCommandTests()
    {
        // Setup MapperMock: map TariffWithThemeDto -> Tariff
        MapperMock.Setup(m => m.Map<Tariff>(It.IsAny<TariffWithThemeDto>()))
            .Returns((TariffWithThemeDto src) => new Tariff
            {
                TariffId = src.TariffId,
                Name = src.Name,
                Description = src.Description,
                PricePerMinute = src.PricePerMinute,
                BillingType = src.BillingType,
                IsActive = src.IsActive
            });

        // Setup MapperMock: map UpdateTariffCommand -> Tariff (apply values to existing Tariff)
        MapperMock.Setup(m => m.Map(It.IsAny<UpdateTariffCommand>(), It.IsAny<Tariff>()))
            .Callback((UpdateTariffCommand src, Tariff dest) =>
            {
                if (!string.IsNullOrWhiteSpace(src.Name)) dest.Name = src.Name;
                if (!string.IsNullOrWhiteSpace(src.Description)) dest.Description = src.Description;
                dest.PricePerMinute = src.PricePerMinute;
                dest.BillingType = src.BillingType;
                dest.IsActive = src.IsActive;
            });

        _handler = new UpdateTariffCommandHandler(TariffRepositoryMock.Object, ThemeRepositoryMock.Object, MapperMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenTariffUpdated()
    {
        var tariffId = TestData.ExistingTariffs.Tariff2Id;
        var tariffDto = new TariffWithThemeDto
        {
            TariffId = tariffId,
            Name = TestData.ExistingTariffs.Tariff2Name,
            Description = "Updated",
            PricePerMinute = TestData.ExistingTariffs.Tariff2PricePerMinute,
            BillingType = BillingType.Hourly,
            IsActive = true
        };
        var tariff = new Tariff
        {
            TariffId = tariffId,
            Name = TestData.ExistingTariffs.Tariff2Name,
            Description = "Updated",
            PricePerMinute = TestData.ExistingTariffs.Tariff2PricePerMinute,
            BillingType = BillingType.Hourly,
            IsActive = true
        };
        var command = new UpdateTariffCommand(
            tariffId.ToString(),
            TestData.ExistingTariffs.Tariff2Name,
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
        result.Tariff!.Name.Should().Be(TestData.ExistingTariffs.Tariff2Name);
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
    public async Task Handler_Should_ThrowCqrsResultException_WhenRepositoryThrowsException()
    {
        var tariffId = TestData.DefaultValues.DefaultTariffId;
        var tariffDto = new TariffWithThemeDto
        {
            TariffId = tariffId,
            Name = TestData.ExistingTariffs.Tariff1Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            BillingType = BillingType.PerMinute
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

        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("UpdateTariffFailed");
        ex.Result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ThrowCqrsResultException_WhenExceptionThrown()
    {
        var tariffId = TestData.DefaultValues.DefaultTariffId;
        var command = new UpdateTariffCommand(
            tariffId.ToString(),
            TestData.ExistingTariffs.Tariff1Name,
            "Desc",
            TestData.ExistingTariffs.Tariff1PricePerMinute,
            BillingType.PerMinute,
            null,
            true);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ThrowsAsync(new Exception());

        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("UpdateTariffFailed");
        ex.Result.StatusCode.Should().Be(500);
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

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenThemeSpecified_ButDoesNotExist()
    {
        var tariffId = TestData.DefaultValues.DefaultTariffId;
        var themeId = TestData.NonExistingIds.NonExistingThemeId;
        var tariffDto = new TariffWithThemeDto
        {
            TariffId = tariffId,
            Name = TestData.DefaultValues.DefaultTariffName,
            Description = "Desc",
            PricePerMinute = TestData.DefaultValues.DefaultTariffPrice,
            BillingType = BillingType.PerMinute,
            IsActive = true
        };
        var command = new UpdateTariffCommand(
            tariffId.ToString(),
            TestData.DefaultValues.DefaultTariffName,
            "Desc",
            TestData.DefaultValues.DefaultTariffPrice,
            BillingType.PerMinute,
            themeId.ToString(),
            true);

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ReturnsAsync(tariffDto);
        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(themeId)).ReturnsAsync((Theme?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("ThemeNotFound");
        result.StatusCode.Should().Be(404);
    }
}
