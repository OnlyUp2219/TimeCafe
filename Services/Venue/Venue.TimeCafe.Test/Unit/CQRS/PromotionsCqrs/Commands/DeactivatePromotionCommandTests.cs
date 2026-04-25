namespace Venue.TimeCafe.Test.Unit.CQRS.PromotionsCqrs.Commands;

public class DeactivatePromotionCommandTests : BaseCqrsHandlerTest
{
    private readonly DeactivatePromotionCommandHandler _handler;

    public DeactivatePromotionCommandTests()
    {
        _handler = new DeactivatePromotionCommandHandler(PromotionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenPromotionDeactivated()
    {
        var promotionId = TestData.ExistingPromotions.Promotion1Id;
        var command = new DeactivatePromotionCommand(promotionId);
        var promotion = new Promotion(promotionId) { Name = TestData.DefaultValues.DefaultPromotionName, Description = TestData.DefaultValues.DefaultPromotionDescription, IsActive = true, ValidFrom = TestData.DateTimeData.GetValidFromDate(), ValidTo = TestData.DateTimeData.GetValidToDate() };

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId, It.IsAny<CancellationToken>())).ReturnsAsync(promotion);
        PromotionRepositoryMock.Setup(r => r.DeactivateAsync(promotionId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenPromotionDoesNotExist()
    {
        var command = new DeactivatePromotionCommand(TestData.NonExistingIds.NonExistingPromotionId);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(TestData.NonExistingIds.NonExistingPromotionId, It.IsAny<CancellationToken>())).ReturnsAsync((Promotion?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsFalse()
    {
        var promotionId = TestData.ExistingPromotions.Promotion1Id;
        var command = new DeactivatePromotionCommand(promotionId);
        var promotion = new Promotion(promotionId) { Name = TestData.ExistingPromotions.Promotion1Name, Description = TestData.ExistingPromotions.Promotion1Description, IsActive = true, ValidFrom = TestData.DateTimeData.GetValidFromDate(), ValidTo = TestData.DateTimeData.GetValidToDate() };

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId, It.IsAny<CancellationToken>())).ReturnsAsync(promotion);
        PromotionRepositoryMock.Setup(r => r.DeactivateAsync(promotionId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var promotionId = TestData.ExistingPromotions.Promotion1Id;
        var command = new DeactivatePromotionCommand(promotionId);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", false, "Акция не найдена")]
    [InlineData("99999999-9999-9999-9999-999999999999", true, null)]
    public async Task Validator_Should_ValidateCorrectly(string promotionIdStr, bool isValid, string? expectedError)
    {
        var command = new DeactivatePromotionCommand(Guid.Parse(promotionIdStr));
        var validator = new DeactivatePromotionCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}

