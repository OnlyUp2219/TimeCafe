namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Queries;

public class GetVisitHistoryQueryTests : BaseCqrsHandlerTest
{
    private readonly GetVisitHistoryQueryHandler _handler;

    public GetVisitHistoryQueryTests()
    {
        _handler = new GetVisitHistoryQueryHandler(VisitRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenHistoryFound()
    {
        var query = new GetVisitHistoryQuery("user123", 1, 10);
        var visits = new List<Visit>
        {
            new() { VisitId = 1, UserId = "user123", TariffId = 1, Status = VisitStatus.Completed },
            new() { VisitId = 2, UserId = "user123", TariffId = 1, Status = VisitStatus.Completed }
        };

        VisitRepositoryMock.Setup(r => r.GetVisitHistoryByUserAsync("user123", 1, 10)).ReturnsAsync(visits);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Visits.Should().NotBeNull();
        result.Visits.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoHistory()
    {
        var query = new GetVisitHistoryQuery("user999", 1, 10);
        var visits = new List<Visit>();

        VisitRepositoryMock.Setup(r => r.GetVisitHistoryByUserAsync("user999", 1, 10)).ReturnsAsync(visits);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Visits.Should().NotBeNull();
        result.Visits.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetVisitHistoryQuery("user123", 1, 10);

        VisitRepositoryMock.Setup(r => r.GetVisitHistoryByUserAsync("user123", 1, 10)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetVisitHistoryFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", 1, 10, false, "ID пользователя обязателен")]
    [InlineData(null, 1, 10, false, "ID пользователя обязателен")]
    [InlineData("user123", 0, 10, false, "Номер страницы должен быть больше 0")]
    [InlineData("user123", -1, 10, false, "Номер страницы должен быть больше 0")]
    [InlineData("user123", 1, 0, false, "Размер страницы должен быть больше 0")]
    [InlineData("user123", 1, -1, false, "Размер страницы должен быть больше 0")]
    [InlineData("user123", 1, 101, false, "Размер страницы не может превышать 100")]
    [InlineData("user123", 1, 10, true, null)]
    [InlineData("user123", 1, 100, true, null)]
    public async Task Validator_Should_ValidateCorrectly(string? userId, int pageNumber, int pageSize, bool isValid, string? expectedError)
    {
        var query = new GetVisitHistoryQuery(userId!, pageNumber, pageSize);
        var validator = new GetVisitHistoryQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
