namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Queries;

using Venue.TimeCafe.Test.Integration.Helpers;

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
        var query = new HasActiveVisitQuery(userId.ToString());

        VisitRepositoryMock.Setup(r => r.HasActiveVisitAsync(userId)).ReturnsAsync(true);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.HasActiveVisit.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFalse_WhenUserHasNoActiveVisit()
    {
        var userId = TestData.NonExistingIds.NonExistingUserId;
        var query = new HasActiveVisitQuery(userId.ToString());

        VisitRepositoryMock.Setup(r => r.HasActiveVisitAsync(userId)).ReturnsAsync(false);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.HasActiveVisit.Should().BeFalse();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var userId = TestData.ExistingVisits.Visit1UserId;
        var query = new HasActiveVisitQuery(userId.ToString());

        VisitRepositoryMock.Setup(r => r.HasActiveVisitAsync(userId)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("CheckActiveVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", true)]
    public async Task Validator_Should_ValidateCorrectly(string? userId, bool isValid)
    {
        var query = new HasActiveVisitQuery(userId!);
        var validator = new HasActiveVisitQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().Be(isValid);
    }
}
