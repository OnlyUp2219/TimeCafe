namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Queries;

public class GetPendingVisitsQueryTests : BaseCqrsHandlerTest
{
    private readonly GetPendingVisitsQueryHandler _handler;

    public GetPendingVisitsQueryTests()
    {
        _handler = new GetPendingVisitsQueryHandler(UowMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenPendingVisitsExist()
    {
        var query = new GetPendingVisitsQuery(1, 20);
        var visits = new List<VisitWithTariffDto>
        {
            new()
            {
                VisitId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TariffId = Guid.NewGuid(),
                EntryTime = DateTimeOffset.UtcNow,
                Status = VisitStatus.Pending
            },
            new()
            {
                VisitId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TariffId = Guid.NewGuid(),
                EntryTime = DateTimeOffset.UtcNow.AddMinutes(-10),
                Status = VisitStatus.Pending
            }
        };

        VisitRepositoryMock.Setup(r => r.GetPendingVisitsAsync(1, 20, It.IsAny<CancellationToken>())).ReturnsAsync(visits);
        VisitRepositoryMock.Setup(r => r.GetPendingCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(visits.Count);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WithEmptyList_WhenNoPendingVisits()
    {
        var query = new GetPendingVisitsQuery(1, 20);

        VisitRepositoryMock.Setup(r => r.GetPendingVisitsAsync(1, 20, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        VisitRepositoryMock.Setup(r => r.GetPendingCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ReturnCorrectPageMetadata_WhenPendingVisitsExist()
    {
        var query = new GetPendingVisitsQuery(2, 5);
        var visits = new List<VisitWithTariffDto>
        {
            new()
            {
                VisitId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TariffId = Guid.NewGuid(),
                EntryTime = DateTimeOffset.UtcNow,
                Status = VisitStatus.Pending
            }
        };

        VisitRepositoryMock.Setup(r => r.GetPendingVisitsAsync(2, 5, It.IsAny<CancellationToken>())).ReturnsAsync(visits);
        VisitRepositoryMock.Setup(r => r.GetPendingCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(11);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Metadata.Page.Should().Be(2);
        result.Value.Metadata.PageSize.Should().Be(5);
        result.Value.Metadata.TotalCount.Should().Be(11);
        result.Value.Metadata.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handler_Should_ReturnEmptyItems_WhenPageNumberExceedsTotalPages()
    {
        var query = new GetPendingVisitsQuery(100, 20);

        VisitRepositoryMock.Setup(r => r.GetPendingVisitsAsync(100, 20, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        VisitRepositoryMock.Setup(r => r.GetPendingCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(5);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.Metadata.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetPendingVisitsQuery(1, 20);

        VisitRepositoryMock.Setup(r => r.GetPendingVisitsAsync(1, 20, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }
}
