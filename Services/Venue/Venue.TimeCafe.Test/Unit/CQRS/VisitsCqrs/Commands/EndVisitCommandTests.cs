using Venue.TimeCafe.Application.CQRS.Visits.Commands;

namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;

public class RequestEndVisitCommandTests : BaseCqrsHandlerTest
{
    private readonly RequestEndVisitCommandHandler _handler;

    public RequestEndVisitCommandTests()
    {
        _handler = new RequestEndVisitCommandHandler(
            UowMock.Object,
            PublisherMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitFinishRequested()
    {
        var visitId = Guid.NewGuid();
        var command = new RequestEndVisitCommand(visitId);
        var visit = new Visit { VisitId = visitId, Status = VisitStatus.Active };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>())).ReturnsAsync((Visit v, CancellationToken _) => v);
        UowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.VisitId.Should().Be(visitId);
        visit.IsFinishRequested.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var visitId = Guid.NewGuid();
        var command = new RequestEndVisitCommand(visitId);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync((Visit?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenVisitNotActive()
    {
        var visitId = Guid.NewGuid();
        var command = new RequestEndVisitCommand(visitId);
        var visit = new Visit { VisitId = visitId, Status = VisitStatus.Pending };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        visit.IsFinishRequested.Should().BeFalse();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", true)]
    public async Task Validator_Should_ValidateCorrectly(string visitIdStr, bool isValid)
    {
        var command = new RequestEndVisitCommand(Guid.Parse(visitIdStr));
        var validator = new RequestEndVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
    }
}
