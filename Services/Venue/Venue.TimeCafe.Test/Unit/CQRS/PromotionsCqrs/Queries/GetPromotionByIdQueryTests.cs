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
        var query = new GetPromotionByIdQuery(promotionId);
        var promotion = new Promotion(promotionId) { Name = TestData.ExistingPromotions.Promotion1Name, Description = TestData.ExistingPromotions.Promotion1Description, ValidFrom = TestData.DateTimeData.GetValidFromDate(), ValidTo = TestData.DateTimeData.GetValidToDate() };

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId, It.IsAny<CancellationToken>())).ReturnsAsync(promotion);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(TestData.ExistingPromotions.Promotion1Name);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenPromotionDoesNotExist()
    {
        var query = new GetPromotionByIdQuery(TestData.NonExistingIds.NonExistingPromotionId);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(TestData.NonExistingIds.NonExistingPromotionId, It.IsAny<CancellationToken>())).ReturnsAsync((Promotion?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var promotionId = TestData.ExistingPromotions.Promotion2Id;
        var query = new GetPromotionByIdQuery(promotionId);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", false, "Акция не найдена")]
    [InlineData("99999999-9999-9999-9999-999999999999", true, null)]
    public async Task Validator_Should_ValidateCorrectly(string promotionIdStr, bool isValid, string? expectedError)
    {
        var query = new GetPromotionByIdQuery(Guid.Parse(promotionIdStr));
        var validator = new GetPromotionByIdQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}

