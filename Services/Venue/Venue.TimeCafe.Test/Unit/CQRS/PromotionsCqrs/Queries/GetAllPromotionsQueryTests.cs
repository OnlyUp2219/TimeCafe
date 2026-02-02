namespace Venue.TimeCafe.Test.Unit.CQRS.PromotionsCqrs.Queries;

public class GetAllPromotionsQueryTests : BaseCqrsHandlerTest
{
    private readonly GetAllPromotionsQueryHandler _handler;

    public GetAllPromotionsQueryTests()
    {
        _handler = new GetAllPromotionsQueryHandler(PromotionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenPromotionsFound()
    {
        var query = new GetAllPromotionsQuery();
        var promotions = new List<Promotion>
        {
            new Promotion(TestData.ExistingPromotions.Promotion1Id) { Name = TestData.ExistingPromotions.Promotion1Name, Description = TestData.ExistingPromotions.Promotion1Description, ValidFrom = TestData.DateTimeData.GetValidFromDate(), ValidTo = TestData.DateTimeData.GetValidToDate() },
            new Promotion(TestData.ExistingPromotions.Promotion2Id) { Name = TestData.ExistingPromotions.Promotion2Name, Description = TestData.ExistingPromotions.Promotion2Description, ValidFrom = TestData.DateTimeData.GetValidFromDate(), ValidTo = TestData.DateTimeData.GetValidToDate() }
        };

        PromotionRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(promotions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Promotions.Should().NotBeNull();
        result.Promotions.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoPromotions()
    {
        var query = new GetAllPromotionsQuery();
        var promotions = new List<Promotion>();

        PromotionRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(promotions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Promotions.Should().NotBeNull();
        result.Promotions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ThrowCqrsResultException_WhenExceptionThrown()
    {
        var query = new GetAllPromotionsQuery();

        PromotionRepositoryMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception());

        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => _handler.Handle(query, CancellationToken.None));

        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("GetPromotionsFailed");
        ex.Result.StatusCode.Should().Be(500);
    }
}
