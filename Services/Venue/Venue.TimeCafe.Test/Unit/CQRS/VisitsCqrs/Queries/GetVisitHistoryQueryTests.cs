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
        var query = new GetVisitHistoryQuery(userId, 1, 10);
        var visits = new List<VisitWithTariffDto>
        {
            new() { VisitId = Guid.NewGuid(), UserId = userId, TariffId = Guid.NewGuid(), Status = VisitStatus.Completed },
            new() { VisitId = Guid.NewGuid(), UserId = userId, TariffId = Guid.NewGuid(), Status = VisitStatus.Completed }
        };

        VisitRepositoryMock.Setup(r => r.GetVisitHistoryByUserAsync(userId, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(visits);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoHistory()
    {
        var userId = TestData.NonExistingIds.NonExistingUserId;
        var query = new GetVisitHistoryQuery(userId, 1, 10);
        var visits = new List<VisitWithTariffDto>();

        VisitRepositoryMock.Setup(r => r.GetVisitHistoryByUserAsync(userId, 1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(visits);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var userId = TestData.ExistingVisits.Visit1UserId;
        var query = new GetVisitHistoryQuery(userId, 1, 10);

        VisitRepositoryMock.Setup(r => r.GetVisitHistoryByUserAsync(userId, 1, 10, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", 1, 10, false)]
    [InlineData("11111111-1111-1111-1111-111111111111", 0, 10, false)]
    [InlineData("11111111-1111-1111-1111-111111111111", 1, 0, false)]
    [InlineData("11111111-1111-1111-1111-111111111111", 1, 101, false)]
    [InlineData("11111111-1111-1111-1111-111111111111", 1, 10, true)]
    [InlineData("11111111-1111-1111-1111-111111111111", 1, 100, true)]
    public async Task Validator_Should_ValidateCorrectly(string userIdStr, int pageNumber, int pageSize, bool isValid)
    {
        var query = new GetVisitHistoryQuery(Guid.Parse(userIdStr), pageNumber, pageSize);
        var validator = new GetVisitHistoryQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().Be(isValid);
    }
}

