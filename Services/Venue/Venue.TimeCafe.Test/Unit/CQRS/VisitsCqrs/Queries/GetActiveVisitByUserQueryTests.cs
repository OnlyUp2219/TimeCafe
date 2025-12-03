namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Queries;

public class GetActiveVisitByUserQueryTests : BaseCqrsHandlerTest
{
    private readonly GetActiveVisitByUserQueryHandler _handler;

    public GetActiveVisitByUserQueryTests()
    {
        _handler = new GetActiveVisitByUserQueryHandler(VisitRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenActiveVisitFound()
    {
        var query = new GetActiveVisitByUserQuery("user123");
        var visit = new Visit
        {
            VisitId = 1,
            UserId = "user123",
            TariffId = 1,
            EntryTime = DateTime.UtcNow,
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.GetActiveVisitByUserAsync("user123")).ReturnsAsync(visit);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Visit.Should().NotBeNull();
        result.Visit!.UserId.Should().Be("user123");
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenNoActiveVisit()
    {
        var query = new GetActiveVisitByUserQuery("user999");

        VisitRepositoryMock.Setup(r => r.GetActiveVisitByUserAsync("user999")).ReturnsAsync((Visit?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("ActiveVisitNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetActiveVisitByUserQuery("user123");

        VisitRepositoryMock.Setup(r => r.GetActiveVisitByUserAsync("user123")).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetActiveVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false, "ID пользователя обязателен")]
    [InlineData(null, false, "ID пользователя обязателен")]
    [InlineData("user123", true, null)]
    public async Task Validator_Should_ValidateCorrectly(string? userId, bool isValid, string? expectedError)
    {
        var query = new GetActiveVisitByUserQuery(userId!);
        var validator = new GetActiveVisitByUserQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
