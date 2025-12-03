namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Queries;

public class GetVisitByIdQueryTests : BaseCqrsHandlerTest
{
    private readonly GetVisitByIdQueryHandler _handler;

    public GetVisitByIdQueryTests()
    {
        _handler = new GetVisitByIdQueryHandler(VisitRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitFound()
    {
        var query = new GetVisitByIdQuery(1);
        var visit = new Visit
        {
            VisitId = 1,
            UserId = "user123",
            TariffId = 1,
            EntryTime = DateTime.UtcNow,
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(visit);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Visit.Should().NotBeNull();
        result.Visit!.UserId.Should().Be("user123");
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var query = new GetVisitByIdQuery(999);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Visit?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("VisitNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetVisitByIdQuery(1);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(0, false, "ID посещения обязателен")]
    [InlineData(-1, false, "ID посещения обязателен")]
    [InlineData(1, true, null)]
    [InlineData(999, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int visitId, bool isValid, string? expectedError)
    {
        var query = new GetVisitByIdQuery(visitId);
        var validator = new GetVisitByIdQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
