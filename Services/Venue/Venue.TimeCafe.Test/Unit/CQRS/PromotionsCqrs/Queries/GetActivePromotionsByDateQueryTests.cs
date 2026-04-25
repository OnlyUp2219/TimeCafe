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
        var date = DateTimeOffset.UtcNow;
        var query = new GetActivePromotionsByDateQuery(date);
        var promotions = new List<Promotion>
        {
            new(TestData.ExistingPromotions.Promotion1Id) { Name = TestData.ExistingPromotions.Promotion1Name, Description = TestData.ExistingPromotions.Promotion1Description, IsActive = true, ValidFrom = date.AddDays(-5), ValidTo = date.AddDays(25) },
            new(TestData.ExistingPromotions.Promotion2Id) { Name = TestData.ExistingPromotions.Promotion2Name, Description = TestData.ExistingPromotions.Promotion2Description, IsActive = true, ValidFrom = date.AddDays(-10), ValidTo = date.AddDays(50) }
        };

        PromotionRepositoryMock.Setup(r => r.GetActiveByDateAsync(date, It.IsAny<CancellationToken>())).ReturnsAsync(promotions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoActivePromotions()
    {
        var date = DateTimeOffset.UtcNow;
        var query = new GetActivePromotionsByDateQuery(date);
        var promotions = new List<Promotion>();

        PromotionRepositoryMock.Setup(r => r.GetActiveByDateAsync(date, It.IsAny<CancellationToken>())).ReturnsAsync(promotions);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var date = DateTimeOffset.UtcNow;
        var query = new GetActivePromotionsByDateQuery(date);

        PromotionRepositoryMock.Setup(r => r.GetActiveByDateAsync(date, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }
}

