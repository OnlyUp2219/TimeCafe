namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Queries;

public class HasActiveVisitQueryTests : BaseCqrsHandlerTest
{
    private readonly HasActiveVisitQueryHandler _handler;

    public HasActiveVisitQueryTests()
    {
        _handler = new HasActiveVisitQueryHandler(VisitRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnTrue_WhenUserHasActiveVisit()
    {
        var userId = TestData.ExistingVisits.Visit1UserId;
        var query = new HasActiveVisitQuery(userId);

        VisitRepositoryMock.Setup(r => r.HasActiveVisitAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFalse_WhenUserHasNoActiveVisit()
    {
        var userId = TestData.NonExistingIds.NonExistingUserId;
        var query = new HasActiveVisitQuery(userId);

        VisitRepositoryMock.Setup(r => r.HasActiveVisitAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var userId = TestData.ExistingVisits.Visit1UserId;
        var query = new HasActiveVisitQuery(userId);

        VisitRepositoryMock.Setup(r => r.HasActiveVisitAsync(userId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", true)]
    public async Task Validator_Should_ValidateCorrectly(string userIdStr, bool isValid)
    {
        var query = new HasActiveVisitQuery(Guid.Parse(userIdStr));
        var validator = new HasActiveVisitQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().Be(isValid);
    }
}

