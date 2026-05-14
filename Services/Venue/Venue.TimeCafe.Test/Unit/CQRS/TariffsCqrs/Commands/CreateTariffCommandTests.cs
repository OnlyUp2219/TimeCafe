namespace Venue.TimeCafe.Test.Unit.CQRS.TariffsCqrs.Commands;

public class CreateTariffCommandTests : BaseCqrsHandlerTest
{
    private readonly CreateTariffCommandHandler _handler;

    public CreateTariffCommandTests()
    {
        _handler = new CreateTariffCommandHandler(UowMock.Object, PublisherMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenTariffCreated()
    {
        var tariffId = Guid.NewGuid();
        var command = new CreateTariffCommand(
            TestData.NewTariffs.NewTariff1Name,
            "Description",
            TestData.NewTariffs.NewTariff1Price,
            TestData.NewTariffs.NewTariff1BillingType,
            null,
            true);
        var tariff = new Tariff
        {
            TariffId = tariffId,
            Name = TestData.NewTariffs.NewTariff1Name,
            Description = "Description",
            PricePerMinute = TestData.NewTariffs.NewTariff1Price,
            BillingType = TestData.NewTariffs.NewTariff1BillingType,
            IsActive = true
        };

        TariffRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Tariff>(), It.IsAny<CancellationToken>())).ReturnsAsync(tariff);
        UowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(TestData.NewTariffs.NewTariff1Name);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryFails()
    {
        var command = new CreateTariffCommand(
            TestData.NewTariffs.NewTariff1Name,
            "Description",
            TestData.NewTariffs.NewTariff1Price,
            TestData.NewTariffs.NewTariff1BillingType,
            null,
            true);

        TariffRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Tariff>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Database error"));

        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var command = new CreateTariffCommand(
            TestData.NewTariffs.NewTariff1Name,
            "Description",
            TestData.NewTariffs.NewTariff1Price,
            TestData.NewTariffs.NewTariff1BillingType,
            null,
            true);

        TariffRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Tariff>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Desc", 10, BillingType.PerMinute, false, "Название тарифа обязательно")]
    [InlineData(null, "Desc", 10, BillingType.PerMinute, false, "Название тарифа обязательно")]
    [InlineData("Valid Name", "Desc", 0, BillingType.PerMinute, false, "Цена должна быть больше 0")]
    [InlineData("Valid Name", "Desc", -1, BillingType.PerMinute, false, "Цена должна быть больше 0")]
    [InlineData("Valid Name", "Desc", 10, BillingType.PerMinute, true, null)]
    public async Task Validator_Should_ValidateCorrectly(string? name, string? description, decimal price, BillingType billingType, bool isValid, string? expectedError)
    {
        var command = new CreateTariffCommand(name!, description, price, billingType, null, true);
        var validator = new CreateTariffCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }

    [Theory]
    [InlineData(-1, null, null, -1, null, null, false, "Минимальное время сессии должно быть больше 0")]
    [InlineData(null, -1, null, -1, null, null, false, "Максимальное количество гостей должно быть больше 0")]
    [InlineData(null, null, "10min", -1, null, null, false, "Неверное правило округления")]
    [InlineData(null, null, null, -1, null, null, false, "Порядок сортировки не может быть отрицательным")]
    [InlineData(null, null, null, 0, "A very long summary that exceeds the two hundred characters limit and thus should fail the validation process because we have set a maximum length for the summary property in our validation extensions...", null, false, "Краткое описание не может превышать 200 символов")]
    [InlineData(null, null, null, 0, null, "A very long cancellation policy that exceeds the five hundred characters limit...", false, "Правила отмены не могут превышать 500 символов")]
    [InlineData(10, 5, "5min", 10, "Summary", "Cancellation", true, null)]
    public async Task Validator_Should_ValidateExtendedFieldsCorrectly(int? minSessionMinutes, int? maxGuests, string? roundingRule, int sortOrder, string? summary, string? cancellationPolicy, bool isValid, string? expectedError)
    {
        if (cancellationPolicy != null && cancellationPolicy.StartsWith("A very long cancellation policy"))
        {
            cancellationPolicy = new string('A', 1001);
        }

        var command = new CreateTariffCommand("Valid Name", "Desc", 10, BillingType.PerMinute, null, true,
            Summary: summary,
            MinSessionMinutes: minSessionMinutes,
            RoundingRule: roundingRule,
            MaxGuests: maxGuests,
            CancellationPolicy: cancellationPolicy,
            SortOrder: sortOrder);

        var validator = new CreateTariffCommandValidator();
        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}

