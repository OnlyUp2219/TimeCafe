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
        var query = new GetPromotionByIdQuery(1);
        var promotion = new Promotion { PromotionId = 1, Name = "Test Promotion", Description = "Desc", ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddDays(30) };

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(promotion);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Promotion.Should().NotBeNull();
        result.Promotion!.Name.Should().Be("Test Promotion");
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenPromotionDoesNotExist()
    {
        var query = new GetPromotionByIdQuery(999);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Promotion?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("PromotionNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetPromotionByIdQuery(1);

        PromotionRepositoryMock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetPromotionFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(0, false, "ID акции обязателен")]
    [InlineData(-1, false, "ID акции обязателен")]
    [InlineData(1, true, null)]
    [InlineData(999, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int promotionId, bool isValid, string? expectedError)
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
