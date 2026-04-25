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

        VisitRepositoryMock.Setup(r => r.GetActiveVisitsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(visits);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoActiveVisits()
    {
        var query = new GetActiveVisitsQuery();
        var visits = new List<VisitWithTariffDto>();

        VisitRepositoryMock.Setup(r => r.GetActiveVisitsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(visits);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetActiveVisitsQuery();

        VisitRepositoryMock.Setup(r => r.GetActiveVisitsAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }
}

