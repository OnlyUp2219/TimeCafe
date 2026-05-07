namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Queries;

public class GetVisitByIdQueryTests : BaseCqrsHandlerTest
{
    private readonly GetVisitByIdQueryHandler _handler;

    public GetVisitByIdQueryTests()
    {
        _handler = new GetVisitByIdQueryHandler(UowMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitFound()
    {
        var visitId = Guid.NewGuid();
        var query = new GetVisitByIdQuery(visitId);
        var visitDto = new VisitWithTariffDto
        {
            VisitId = visitId,
            UserId = TestData.ExistingVisits.Visit1UserId,
            TariffId = Guid.NewGuid(),
            EntryTime = DateTimeOffset.UtcNow,
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.GetWithTariffByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visitDto);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(TestData.ExistingVisits.Visit1UserId);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var visitId = TestData.NonExistingIds.NonExistingVisitId;
        var query = new GetVisitByIdQuery(visitId);

        VisitRepositoryMock.Setup(r => r.GetWithTariffByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync((VisitWithTariffDto?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var visitId = Guid.NewGuid();
        var query = new GetVisitByIdQuery(visitId);

        VisitRepositoryMock.Setup(r => r.GetWithTariffByIdAsync(visitId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }

}

