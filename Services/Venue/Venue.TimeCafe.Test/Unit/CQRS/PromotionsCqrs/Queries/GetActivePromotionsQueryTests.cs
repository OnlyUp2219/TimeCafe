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
            new Promotion(TestData.ExistingPromotions.Promotion1Id) { Name = TestData.ExistingPromotions.Promotion1Name, Description = TestData.ExistingPromotions.Promotion1Description, IsActive = true, ValidFrom = TestData.DateTimeData.GetValidFromDate(), ValidTo = TestData.DateTimeData.GetValidToDate() },
            new Promotion(TestData.ExistingPromotions.Promotion2Id) { Name = TestData.ExistingPromotions.Promotion2Name, Description = TestData.ExistingPromotions.Promotion2Description, IsActive = true, ValidFrom = TestData.DateTimeData.GetValidFromDate(), ValidTo = TestData.DateTimeData.GetValidToDate() }
        };

        PromotionRepositoryMock.Setup(r => r.GetActiveAsync()).ReturnsAsync(promotions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Promotions.Should().NotBeNull();
        result.Promotions.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoActivePromotions()
    {
        var query = new GetActivePromotionsQuery();
        var promotions = new List<Promotion>();

        PromotionRepositoryMock.Setup(r => r.GetActiveAsync()).ReturnsAsync(promotions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Promotions.Should().NotBeNull();
        result.Promotions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ThrowCqrsResultException_WhenExceptionThrown()
    {
        var query = new GetActivePromotionsQuery();

        PromotionRepositoryMock.Setup(r => r.GetActiveAsync()).ThrowsAsync(new Exception());

        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => _handler.Handle(query, CancellationToken.None));

        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("GetActivePromotionsFailed");
        ex.Result.StatusCode.Should().Be(500);
    }
}
