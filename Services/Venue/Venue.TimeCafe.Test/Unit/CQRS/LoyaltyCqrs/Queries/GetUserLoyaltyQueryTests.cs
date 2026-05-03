namespace Venue.TimeCafe.Test.Unit.CQRS.LoyaltyCqrs.Queries;

public class GetUserLoyaltyQueryTests : BaseCqrsHandlerTest
{
    private readonly GetUserLoyaltyQueryHandler _handler;

    public GetUserLoyaltyQueryTests()
    {
        _handler = new GetUserLoyaltyQueryHandler(UserLoyaltyRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnLoyalty_WhenExists()
    {
        var userId = Guid.NewGuid();
        var loyalty = new UserLoyalty(userId) { PersonalDiscountPercent = 10m };
        UserLoyaltyRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loyalty);

        var result = await _handler.Handle(new GetUserLoyaltyQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.PersonalDiscountPercent.Should().Be(10m);
    }

    [Fact]
    public async Task Handler_Should_ReturnZeroDiscount_WhenNotFound()
    {
        var userId = Guid.NewGuid();
        UserLoyaltyRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserLoyalty?)null);

        var result = await _handler.Handle(new GetUserLoyaltyQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.PersonalDiscountPercent.Should().Be(0m);
    }
}
