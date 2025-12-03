namespace Venue.TimeCafe.Test.Unit.CQRS.PromotionsCqrs.Commands;

public class UpdatePromotionCommandTests : BaseCqrsHandlerTest
{
    private readonly UpdatePromotionCommandHandler _handler;

    public UpdatePromotionCommandTests()
    {
        _handler = new UpdatePromotionCommandHandler(PromotionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenPromotionUpdated()
    {
        var validFrom = DateTime.UtcNow;
        var validTo = validFrom.AddDays(30);
        var promotion = new Promotion
        {
            PromotionId = 1,
            Name = "Updated Promotion",
            Description = "Updated Description",
            DiscountPercent = 15m,
            ValidFrom = validFrom,
            ValidTo = validTo,
            IsActive = true
        };
        var command = new UpdatePromotionCommand(promotion);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(promotion);
        PromotionRepositoryMock.Setup(r => r.UpdateAsync(promotion)).ReturnsAsync(promotion);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Promotion.Should().NotBeNull();
        result.Promotion!.Name.Should().Be("Updated Promotion");
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenPromotionDoesNotExist()
    {
        var validFrom = DateTime.UtcNow;
        var validTo = validFrom.AddDays(30);
        var promotion = new Promotion
        {
            PromotionId = 999,
            Name = "Nonexistent",
            Description = "Desc",
            ValidFrom = validFrom,
            ValidTo = validTo
        };
        var command = new UpdatePromotionCommand(promotion);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Promotion?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("PromotionNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenUpdateFails()
    {
        var validFrom = DateTime.UtcNow;
        var validTo = validFrom.AddDays(30);
        var promotion = new Promotion
        {
            PromotionId = 1,
            Name = "Test",
            Description = "Desc",
            ValidFrom = validFrom,
            ValidTo = validTo
        };
        var command = new UpdatePromotionCommand(promotion);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(promotion);
        PromotionRepositoryMock.Setup(r => r.UpdateAsync(promotion)).ThrowsAsync(new Exception("Update failed"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("UpdatePromotionFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var validFrom = DateTime.UtcNow;
        var validTo = validFrom.AddDays(30);
        var promotion = new Promotion
        {
            PromotionId = 1,
            Name = "Test",
            Description = "Desc",
            ValidFrom = validFrom,
            ValidTo = validTo
        };
        var command = new UpdatePromotionCommand(promotion);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("UpdatePromotionFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(null, "Name", "Desc", 10.0, true, false, "Акция обязательна")]
    [InlineData(0, "Name", "Desc", 10.0, true, false, "ID акции обязателен")]
    [InlineData(-1, "Name", "Desc", 10.0, true, false, "ID акции обязателен")]
    [InlineData(1, "", "Desc", 10.0, true, false, "Название акции обязательно")]
    [InlineData(1, null, "Desc", 10.0, true, false, "Название акции обязательно")]
    [InlineData(1, "Name", "", 10.0, true, false, "Описание акции обязательно")]
    [InlineData(1, "Name", null, 10.0, true, false, "Описание акции обязательно")]
    [InlineData(1, "Name", "Desc", 0.0, true, false, "Процент скидки должен быть больше 0")]
    [InlineData(1, "Name", "Desc", -1.0, true, false, "Процент скидки должен быть больше 0")]
    [InlineData(1, "Name", "Desc", 101.0, true, false, "Процент скидки не может превышать 100")]
    [InlineData(1, "Name", "Desc", 10.0, false, false, "Дата начала должна быть раньше даты окончания")]
    [InlineData(1, "Name", "Desc", 10.0, true, true, null)]
    [InlineData(1, "Name", "Desc", -999.0, true, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int? promotionId, string? name, string? description, double discountDouble, bool validDates, bool isValid, string? expectedError)
    {
        var validFrom = DateTime.UtcNow;
        var validTo = validDates ? validFrom.AddDays(30) : validFrom.AddDays(-1);
        var discount = discountDouble == -999.0 ? null : (decimal?)discountDouble;
        var promotion = promotionId.HasValue ? new Promotion
        {
            PromotionId = promotionId.Value,
            Name = name!,
            Description = description!,
            DiscountPercent = discount,
            ValidFrom = validFrom,
            ValidTo = validTo
        } : null;
        var command = new UpdatePromotionCommand(promotion!);
        var validator = new UpdatePromotionCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
