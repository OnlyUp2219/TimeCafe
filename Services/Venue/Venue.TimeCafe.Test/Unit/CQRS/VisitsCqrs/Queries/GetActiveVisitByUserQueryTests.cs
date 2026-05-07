namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Queries;

public class GetActiveVisitByUserQueryTests : BaseCqrsHandlerTest
{
    private readonly GetActiveVisitByUserQueryHandler _handler;

    public GetActiveVisitByUserQueryTests()
    {
        _handler = new GetActiveVisitByUserQueryHandler(UowMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenActiveVisitFound()
    {
        var userId = TestData.ExistingVisits.Visit1UserId;
        var query = new GetActiveVisitByUserQuery(userId);
        var visitDto = new VisitWithTariffDto
        {
            VisitId = Guid.NewGuid(),
            UserId = userId,
            TariffId = Guid.NewGuid(),
            EntryTime = DateTimeOffset.UtcNow,
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.GetActiveVisitByUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(visitDto);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenNoActiveVisit()
    {
        var userId = TestData.NonExistingIds.NonExistingUserId;
        var query = new GetActiveVisitByUserQuery(userId);

        VisitRepositoryMock.Setup(r => r.GetActiveVisitByUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((VisitWithTariffDto?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var userId = TestData.ExistingVisits.Visit1UserId;
        var query = new GetActiveVisitByUserQuery(userId);

        VisitRepositoryMock.Setup(r => r.GetActiveVisitByUserAsync(userId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }

}

