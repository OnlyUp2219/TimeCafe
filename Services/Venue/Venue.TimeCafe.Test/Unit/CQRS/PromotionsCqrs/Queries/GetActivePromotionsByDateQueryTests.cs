namespace Venue.TimeCafe.Test.Unit.CQRS.PromotionsCqrs.Queries;

public class GetActivePromotionsByDateQueryTests : BaseCqrsHandlerTest
{
    private readonly GetActivePromotionsByDateQueryHandler _handler;

    public GetActivePromotionsByDateQueryTests()
    {
        _handler = new GetActivePromotionsByDateQueryHandler(PromotionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenActivePromotionsFound()
    {
        var date = DateTime.UtcNow;
        var query = new GetActivePromotionsByDateQuery(date);
        var promotions = new List<Promotion>
        {
            new() { PromotionId = 1, Name = "Promotion 1", Description = "Desc 1", IsActive = true, ValidFrom = date.AddDays(-5), ValidTo = date.AddDays(25) },
            new() { PromotionId = 2, Name = "Promotion 2", Description = "Desc 2", IsActive = true, ValidFrom = date.AddDays(-10), ValidTo = date.AddDays(50) }
        };

        PromotionRepositoryMock.Setup(r => r.GetActiveByDateAsync(date)).ReturnsAsync(promotions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Promotions.Should().NotBeNull();
        result.Promotions.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoActivePromotions()
    {
        var date = DateTime.UtcNow;
        var query = new GetActivePromotionsByDateQuery(date);
        var promotions = new List<Promotion>();

        PromotionRepositoryMock.Setup(r => r.GetActiveByDateAsync(date)).ReturnsAsync(promotions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Promotions.Should().NotBeNull();
        result.Promotions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var date = DateTime.UtcNow;
        var query = new GetActivePromotionsByDateQuery(date);

        PromotionRepositoryMock.Setup(r => r.GetActiveByDateAsync(date)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetActivePromotionsByDateFailed");
        result.StatusCode.Should().Be(500);
    }
}
