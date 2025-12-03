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
            new() { PromotionId = 1, Name = "Promotion 1", Description = "Desc 1", IsActive = true, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddDays(30) },
            new() { PromotionId = 2, Name = "Promotion 2", Description = "Desc 2", IsActive = true, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddDays(60) }
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
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetActivePromotionsQuery();

        PromotionRepositoryMock.Setup(r => r.GetActiveAsync()).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetActivePromotionsFailed");
        result.StatusCode.Should().Be(500);
    }
}
