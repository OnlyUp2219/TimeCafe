namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Queries;

public class GetActiveVisitsQueryTests : BaseCqrsHandlerTest
{
    private readonly GetActiveVisitsQueryHandler _handler;

    public GetActiveVisitsQueryTests()
    {
        _handler = new GetActiveVisitsQueryHandler(VisitRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenActiveVisitsFound()
    {
        var query = new GetActiveVisitsQuery();
        var visits = new List<VisitWithTariffDto>
        {
            new() { VisitId = Guid.NewGuid(), UserId = TestData.ExistingVisits.Visit1UserId, TariffId = Guid.NewGuid(), Status = VisitStatus.Active },
            new() { VisitId = Guid.NewGuid(), UserId = TestData.ExistingVisits.Visit2UserId, TariffId = Guid.NewGuid(), Status = VisitStatus.Active }
        };

        VisitRepositoryMock.Setup(r => r.GetActiveVisitsAsync()).ReturnsAsync(visits);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Visits.Should().NotBeNull();
        result.Visits.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoActiveVisits()
    {
        var query = new GetActiveVisitsQuery();
        var visits = new List<VisitWithTariffDto>();

        VisitRepositoryMock.Setup(r => r.GetActiveVisitsAsync()).ReturnsAsync(visits);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Visits.Should().NotBeNull();
        result.Visits.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ThrowCqrsResultException_WhenExceptionThrown()
    {
        var query = new GetActiveVisitsQuery();

        VisitRepositoryMock.Setup(r => r.GetActiveVisitsAsync()).ThrowsAsync(new Exception());

        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => _handler.Handle(query, CancellationToken.None));

        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("GetActiveVisitsFailed");
        ex.Result.StatusCode.Should().Be(500);
    }
}
