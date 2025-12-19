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
        var userId = TestData.ExistingVisits.Visit1UserId;
        var query = new GetVisitHistoryQuery(userId.ToString(), 1, 10);
        var visits = new List<VisitWithTariffDto>
        {
            new() { VisitId = Guid.NewGuid(), UserId = userId, TariffId = Guid.NewGuid(), Status = VisitStatus.Completed },
            new() { VisitId = Guid.NewGuid(), UserId = userId, TariffId = Guid.NewGuid(), Status = VisitStatus.Completed }
        };

        VisitRepositoryMock.Setup(r => r.GetVisitHistoryByUserAsync(userId, 1, 10)).ReturnsAsync(visits);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Visits.Should().NotBeNull();
        result.Visits.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoHistory()
    {
        var userId = TestData.NonExistingIds.NonExistingUserId;
        var query = new GetVisitHistoryQuery(userId.ToString(), 1, 10);
        var visits = new List<VisitWithTariffDto>();

        VisitRepositoryMock.Setup(r => r.GetVisitHistoryByUserAsync(userId, 1, 10)).ReturnsAsync(visits);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Visits.Should().NotBeNull();
        result.Visits.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var userId = TestData.ExistingVisits.Visit1UserId;
        var query = new GetVisitHistoryQuery(userId.ToString(), 1, 10);

        VisitRepositoryMock.Setup(r => r.GetVisitHistoryByUserAsync(userId, 1, 10)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetVisitHistoryFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", 1, 10, false)]
    [InlineData("not-a-guid", 1, 10, false)]
    [InlineData("11111111-1111-1111-1111-111111111111", 0, 10, false)]
    [InlineData("11111111-1111-1111-1111-111111111111", 1, 0, false)]
    [InlineData("11111111-1111-1111-1111-111111111111", 1, 101, false)]
    [InlineData("11111111-1111-1111-1111-111111111111", 1, 10, true)]
    [InlineData("11111111-1111-1111-1111-111111111111", 1, 100, true)]
    public async Task Validator_Should_ValidateCorrectly(string? userId, int pageNumber, int pageSize, bool isValid)
    {
        var query = new GetVisitHistoryQuery(userId!, pageNumber, pageSize);
        var validator = new GetVisitHistoryQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().Be(isValid);
    }
}
