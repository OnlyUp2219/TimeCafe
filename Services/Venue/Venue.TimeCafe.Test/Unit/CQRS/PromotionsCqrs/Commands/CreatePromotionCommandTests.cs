namespace Venue.TimeCafe.Test.Unit.CQRS.PromotionsCqrs.Commands;

public class CreatePromotionCommandTests : BaseCqrsHandlerTest
{
    private readonly CreatePromotionCommandHandler _handler;

    public CreatePromotionCommandTests()
    {
        _handler = new CreatePromotionCommandHandler(PromotionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenPromotionCreated()
    {
        var validFrom = DateTime.UtcNow;
        var validTo = validFrom.AddDays(30);
        var command = new CreatePromotionCommand("Test Promotion", "Description", 10m, validFrom, validTo, true);
        var promotion = new Promotion
        {
            PromotionId = 1,
            Name = "Test Promotion",
            Description = "Description",
            DiscountPercent = 10m,
            ValidFrom = validFrom,
            ValidTo = validTo,
            IsActive = true
        };

        PromotionRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Promotion>())).ReturnsAsync(promotion);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Promotion.Should().NotBeNull();
        result.Promotion!.Name.Should().Be("Test Promotion");
        result.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryFails()
    {
        var validFrom = DateTime.UtcNow;
        var validTo = validFrom.AddDays(30);
        var command = new CreatePromotionCommand("Test", "Desc", 10m, validFrom, validTo);

        PromotionRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Promotion>())).ThrowsAsync(new InvalidOperationException());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreatePromotionFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var validFrom = DateTime.UtcNow;
        var validTo = validFrom.AddDays(30);
        var command = new CreatePromotionCommand("Test", "Desc", 10m, validFrom, validTo);

        PromotionRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Promotion>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreatePromotionFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", "Desc", 10.0, true, false, "Название акции обязательно")]
    [InlineData(null, "Desc", 10.0, true, false, "Название акции обязательно")]
    [InlineData("Name", "", 10.0, true, false, "Описание акции обязательно")]
    [InlineData("Name", null, 10.0, true, false, "Описание акции обязательно")]
    [InlineData("Name", "Desc", 0.0, true, false, "Процент скидки должен быть больше 0")]
    [InlineData("Name", "Desc", -1.0, true, false, "Процент скидки должен быть больше 0")]
    [InlineData("Name", "Desc", 101.0, true, false, "Процент скидки не может превышать 100")]
    [InlineData("Name", "Desc", 10.0, false, false, "Дата начала должна быть раньше даты окончания")]
    [InlineData("Name", "Desc", 10.0, true, true, null)]
    [InlineData("Name", "Desc", -999.0, true, true, null)]
    public async Task Validator_Should_ValidateCorrectly(string? name, string? description, double discountDouble, bool validDates, bool isValid, string? expectedError)
    {
        var validFrom = DateTime.UtcNow;
        var validTo = validDates ? validFrom.AddDays(30) : validFrom.AddDays(-1);
        var discount = discountDouble == -999.0 ? null : (decimal?)discountDouble;
        var command = new CreatePromotionCommand(name!, description!, discount, validFrom, validTo);
        var validator = new CreatePromotionCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
