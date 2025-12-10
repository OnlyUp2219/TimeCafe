namespace Venue.TimeCafe.Test.Unit.CQRS.PromotionsCqrs.Queries;

public class GetPromotionByIdQueryTests : BaseCqrsHandlerTest
{
    private readonly GetPromotionByIdQueryHandler _handler;

    public GetPromotionByIdQueryTests()
    {
        _handler = new GetPromotionByIdQueryHandler(PromotionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenPromotionFound()
    {
        var promotionId = TestData.ExistingPromotions.Promotion1Id;
        var query = new GetPromotionByIdQuery(promotionId.ToString());
        var promotion = new Promotion(promotionId) { Name = TestData.ExistingPromotions.Promotion1Name, Description = TestData.ExistingPromotions.Promotion1Description, ValidFrom = TestData.DateTimeData.GetValidFromDate(), ValidTo = TestData.DateTimeData.GetValidToDate() };

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId)).ReturnsAsync(promotion);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Promotion.Should().NotBeNull();
        result.Promotion!.Name.Should().Be(TestData.ExistingPromotions.Promotion1Name);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenPromotionDoesNotExist()
    {
        var query = new GetPromotionByIdQuery(TestData.NonExistingIds.NonExistingPromotionIdString);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(TestData.NonExistingIds.NonExistingPromotionId)).ReturnsAsync((Promotion?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("PromotionNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var promotionId = TestData.ExistingPromotions.Promotion2Id;
        var query = new GetPromotionByIdQuery(promotionId.ToString());

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetPromotionFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false, "Акция не найдена")]
    [InlineData("not-a-guid", false, "Акция не найдена")]
    [InlineData("00000000-0000-0000-0000-000000000000", true, null)]
    [InlineData("99999999-9999-9999-9999-999999999999", true, null)]
    public async Task Validator_Should_ValidateCorrectly(string promotionId, bool isValid, string? expectedError)
    {
        var query = new GetPromotionByIdQuery(promotionId);
        var validator = new GetPromotionByIdQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
