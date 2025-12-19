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
        var userId = TestData.ExistingVisits.Visit1UserId;
        var query = new GetActiveVisitByUserQuery(userId.ToString());
        var visitDto = new VisitWithTariffDto
        {
            VisitId = Guid.NewGuid(),
            UserId = userId,
            TariffId = Guid.NewGuid(),
            EntryTime = DateTimeOffset.UtcNow,
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.GetActiveVisitByUserAsync(userId)).ReturnsAsync(visitDto);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Visit.Should().NotBeNull();
        result.Visit!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenNoActiveVisit()
    {
        var userId = TestData.NonExistingIds.NonExistingUserId;
        var query = new GetActiveVisitByUserQuery(userId.ToString());

        VisitRepositoryMock.Setup(r => r.GetActiveVisitByUserAsync(userId)).ReturnsAsync((VisitWithTariffDto?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("ActiveVisitNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var userId = TestData.ExistingVisits.Visit1UserId;
        var query = new GetActiveVisitByUserQuery(userId.ToString());

        VisitRepositoryMock.Setup(r => r.GetActiveVisitByUserAsync(userId)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetActiveVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("not-a-guid", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", true)]
    public async Task Validator_Should_ValidateCorrectly(string? userId, bool isValid)
    {
        var query = new GetActiveVisitByUserQuery(userId!);
        var validator = new GetActiveVisitByUserQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().Be(isValid);
    }
}
