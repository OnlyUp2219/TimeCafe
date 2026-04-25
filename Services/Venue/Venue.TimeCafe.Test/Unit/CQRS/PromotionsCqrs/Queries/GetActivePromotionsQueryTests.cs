namespace Venue.TimeCafe.Test.Unit.CQRS.PromotionsCqrs.Queries;

public class GetActivePromotionsQueryTests : BaseCqrsHandlerTest
{
    private readonly GetActivePromotionsQueryHandler _handler;

    public GetActivePromotionsQueryTests()
    {
        _handler = new GetActivePromotionsQueryHandler(PromotionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenActivePromotionsFound()
    {
        var query = new GetActivePromotionsQuery();
        var promotions = new List<Promotion>
        {
            new(TestData.ExistingPromotions.Promotion1Id) { Name = TestData.ExistingPromotions.Promotion1Name, Description = TestData.ExistingPromotions.Promotion1Description, IsActive = true, ValidFrom = TestData.DateTimeData.GetValidFromDate(), ValidTo = TestData.DateTimeData.GetValidToDate() },
            new(TestData.ExistingPromotions.Promotion2Id) { Name = TestData.ExistingPromotions.Promotion2Name, Description = TestData.ExistingPromotions.Promotion2Description, IsActive = true, ValidFrom = TestData.DateTimeData.GetValidFromDate(), ValidTo = TestData.DateTimeData.GetValidToDate() }
        };

        PromotionRepositoryMock.Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(promotions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoActivePromotions()
    {
        var query = new GetActivePromotionsQuery();
        var promotions = new List<Promotion>();

        PromotionRepositoryMock.Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(promotions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetActivePromotionsQuery();

        PromotionRepositoryMock.Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }
}

