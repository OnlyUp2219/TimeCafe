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
        var command = new DeactivatePromotionCommand(promotionId.ToString());
        var promotion = new Promotion(promotionId) { Name = TestData.DefaultValues.DefaultPromotionName, Description = TestData.DefaultValues.DefaultPromotionDescription, IsActive = true, ValidFrom = TestData.DateTimeData.GetValidFromDate(), ValidTo = TestData.DateTimeData.GetValidToDate() };

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId)).ReturnsAsync(promotion);
        PromotionRepositoryMock.Setup(r => r.DeactivateAsync(promotionId)).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().BeNull();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenPromotionDoesNotExist()
    {
        var command = new DeactivatePromotionCommand(TestData.NonExistingIds.NonExistingPromotionIdString);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(TestData.NonExistingIds.NonExistingPromotionId)).ReturnsAsync((Promotion?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("PromotionNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsFalse()
    {
        var promotionId = TestData.ExistingPromotions.Promotion1Id;
        var command = new DeactivatePromotionCommand(promotionId.ToString());
        var promotion = new Promotion(promotionId) { Name = TestData.ExistingPromotions.Promotion1Name, Description = TestData.ExistingPromotions.Promotion1Description, IsActive = true, ValidFrom = TestData.DateTimeData.GetValidFromDate(), ValidTo = TestData.DateTimeData.GetValidToDate() };

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId)).ReturnsAsync(promotion);
        PromotionRepositoryMock.Setup(r => r.DeactivateAsync(promotionId)).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeactivatePromotionFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var promotionId = TestData.ExistingPromotions.Promotion1Id;
        var command = new DeactivatePromotionCommand(promotionId.ToString());

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeactivatePromotionFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false, "Акция не найдена")]
    [InlineData("not-a-guid", false, "Акция не найдена")]
    [InlineData("00000000-0000-0000-0000-000000000000", false, "Акция не найдена")]
    [InlineData("99999999-9999-9999-9999-999999999999", true, null)]
    public async Task Validator_Should_ValidateCorrectly(string promotionId, bool isValid, string? expectedError)
    {
        var command = new DeactivatePromotionCommand(promotionId);
        var validator = new DeactivatePromotionCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
