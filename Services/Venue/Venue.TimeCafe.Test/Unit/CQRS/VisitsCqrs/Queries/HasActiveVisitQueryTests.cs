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
        var query = new HasActiveVisitQuery("user123");

        VisitRepositoryMock.Setup(r => r.HasActiveVisitAsync("user123")).ReturnsAsync(true);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.HasActiveVisit.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFalse_WhenUserHasNoActiveVisit()
    {
        var query = new HasActiveVisitQuery("user999");

        VisitRepositoryMock.Setup(r => r.HasActiveVisitAsync("user999")).ReturnsAsync(false);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.HasActiveVisit.Should().BeFalse();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new HasActiveVisitQuery("user123");

        VisitRepositoryMock.Setup(r => r.HasActiveVisitAsync("user123")).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("CheckActiveVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false, "ID пользователя обязателен")]
    [InlineData(null, false, "ID пользователя обязателен")]
    [InlineData("user123", true, null)]
    public async Task Validator_Should_ValidateCorrectly(string? userId, bool isValid, string? expectedError)
    {
        var query = new HasActiveVisitQuery(userId!);
        var validator = new HasActiveVisitQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
