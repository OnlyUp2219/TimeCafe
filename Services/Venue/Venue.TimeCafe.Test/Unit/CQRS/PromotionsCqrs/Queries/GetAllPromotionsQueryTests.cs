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
            new() { PromotionId = 1, Name = "Promotion 1", Description = "Desc 1", ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddDays(30) },
            new() { PromotionId = 2, Name = "Promotion 2", Description = "Desc 2", ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddDays(60) }
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
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetAllPromotionsQuery();

        PromotionRepositoryMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetPromotionsFailed");
        result.StatusCode.Should().Be(500);
    }
}
