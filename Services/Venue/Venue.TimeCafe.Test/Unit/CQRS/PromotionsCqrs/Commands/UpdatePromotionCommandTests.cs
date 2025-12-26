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
        var promotionId = TestData.ExistingPromotions.Promotion1Id;
        var validFrom = TestData.DateTimeData.GetValidFromDate();
        var validTo = TestData.DateTimeData.GetValidToDate();
        var promotion = new Promotion(promotionId)
        {
            Name = TestData.UpdateData.UpdatedPromotionName,
            Description = TestData.UpdateData.UpdatedPromotionDescription,
            DiscountPercent = TestData.UpdateData.UpdatedDiscountPercent,
            ValidFrom = validFrom,
            ValidTo = validTo,
            IsActive = true
        };
        var command = new UpdatePromotionCommand(promotionId.ToString(), TestData.UpdateData.UpdatedPromotionName, TestData.UpdateData.UpdatedPromotionDescription, TestData.UpdateData.UpdatedDiscountPercent, validFrom, validTo, true);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId)).ReturnsAsync(promotion);
        PromotionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Promotion>())).ReturnsAsync(promotion);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Promotion.Should().NotBeNull();
        result.Promotion!.Name.Should().Be(TestData.UpdateData.UpdatedPromotionName);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenPromotionDoesNotExist()
    {
        var validFrom = TestData.DateTimeData.GetValidFromDate();
        var validTo = TestData.DateTimeData.GetValidToDate();
        var command = new UpdatePromotionCommand(TestData.NonExistingIds.NonExistingPromotionIdString, TestData.ExistingPromotions.Promotion1Name, TestData.DefaultValues.DefaultPromotionDescription, 10m, validFrom, validTo, true);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(TestData.NonExistingIds.NonExistingPromotionId)).ReturnsAsync((Promotion?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("PromotionNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenUpdateFails()
    {
        var promotionId = TestData.ExistingPromotions.Promotion2Id;
        var validFrom = TestData.DateTimeData.GetValidFromDate();
        var validTo = TestData.DateTimeData.GetValidToDate();
        var promotion = new Promotion(promotionId)
        {
            Name = TestData.DefaultValues.DefaultPromotionName,
            Description = TestData.DefaultValues.DefaultPromotionDescription,
            ValidFrom = validFrom,
            ValidTo = validTo
        };
        var command = new UpdatePromotionCommand(promotionId.ToString(), TestData.DefaultValues.DefaultPromotionName, TestData.DefaultValues.DefaultPromotionDescription, 10m, validFrom, validTo, true);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId)).ReturnsAsync(promotion);
        PromotionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Promotion>())).ThrowsAsync(new Exception("Update failed"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("UpdatePromotionFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var promotionId = TestData.ExistingPromotions.Promotion3Id;
        var validFrom = TestData.DateTimeData.GetValidFromDate();
        var validTo = TestData.DateTimeData.GetValidToDate();
        var command = new UpdatePromotionCommand(promotionId.ToString(), TestData.DefaultValues.DefaultPromotionName, TestData.DefaultValues.DefaultPromotionDescription, 10m, validFrom, validTo, true);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("UpdatePromotionFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", "Name", "Desc", 10.0, true, false, "Акция не найдена")]
    [InlineData("not-a-guid", "Name", "Desc", 10.0, true, false, "Акция не найдена")]
    [InlineData("99999999-9999-9999-9999-999999999999", "", "Desc", 10.0, true, false, "Название акции обязательно")]
    [InlineData("99999999-9999-9999-9999-999999999999", null, "Desc", 10.0, true, false, "Название акции обязательно")]
    [InlineData("99999999-9999-9999-9999-999999999999", "Name", "", 10.0, true, false, "Описание акции обязательно")]
    [InlineData("99999999-9999-9999-9999-999999999999", "Name", null, 10.0, true, false, "Описание акции обязательно")]
    [InlineData("99999999-9999-9999-9999-999999999999", "Name", "Desc", 0.0, true, false, "Процент скидки должен быть больше 0")]
    [InlineData("99999999-9999-9999-9999-999999999999", "Name", "Desc", -1.0, true, false, "Процент скидки должен быть больше 0")]
    [InlineData("99999999-9999-9999-9999-999999999999", "Name", "Desc", 101.0, true, false, "Процент скидки не может превышать 100")]
    [InlineData("99999999-9999-9999-9999-999999999999", "Name", "Desc", 10.0, false, false, "Дата начала должна быть раньше даты окончания")]
    [InlineData("99999999-9999-9999-9999-999999999999", "Name", "Desc", 10.0, true, true, null)]
    [InlineData("99999999-9999-9999-9999-999999999999", "Name", "Desc", -999.0, true, true, null)]
    public async Task Validator_Should_ValidateCorrectly(string promotionId, string? name, string? description, double discountDouble, bool validDates, bool isValid, string? expectedError)
    {
        var validFrom = DateTimeOffset.UtcNow;
        var validTo = validDates ? validFrom.AddDays(30) : validFrom.AddDays(-1);
        var discount = discountDouble == -999.0 ? null : (decimal?)discountDouble;
        var command = new UpdatePromotionCommand(promotionId, name!, description!, discount, validFrom, validTo, true);
        var validator = new UpdatePromotionCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
