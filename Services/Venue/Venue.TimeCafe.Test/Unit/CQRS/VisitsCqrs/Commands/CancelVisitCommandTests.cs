namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;

public class CancelVisitCommandTests : BaseCqrsHandlerTest
{
    private readonly CancelVisitCommandHandler _handler;

    public CancelVisitCommandTests()
    {
        _handler = new CancelVisitCommandHandler(UowMock.Object, PublisherMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitCancelled()
    {
        var visitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CancelVisitCommand(visitId, userId);
        var visit = new Visit
        {
            VisitId = visitId,
            UserId = userId,
            TariffId = Guid.NewGuid(),
            Status = VisitStatus.Pending
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>())).ReturnsAsync((Visit v, CancellationToken _) => v);
        UowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var visitId = TestData.NonExistingIds.NonExistingVisitId;
        var command = new CancelVisitCommand(visitId, Guid.NewGuid());

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync((Visit?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenVisitNotPending()
    {
        var visitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CancelVisitCommand(visitId, userId);
        var visit = new Visit
        {
            VisitId = visitId,
            UserId = userId,
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsNull()
    {
        var visitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CancelVisitCommand(visitId, userId);
        var visit = new Visit
        {
            VisitId = visitId,
            UserId = userId,
            Status = VisitStatus.Pending
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>())).ReturnsAsync((Visit?)null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var visitId = Guid.NewGuid();
        var command = new CancelVisitCommand(visitId, Guid.NewGuid());

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenUserDoesNotOwnVisit()
    {
        var visitId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var command = new CancelVisitCommand(visitId, otherUserId);
        var visit = new Visit
        {
            VisitId = visitId,
            UserId = ownerId,
            Status = VisitStatus.Pending
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Metadata.Should().ContainKey("ErrorCode").WhoseValue.Should().Be("403");
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenVisitAlreadyCancelled()
    {
        var visitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CancelVisitCommand(visitId, userId);
        var visit = new Visit
        {
            VisitId = visitId,
            UserId = userId,
            Status = VisitStatus.Cancelled
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenVisitAlreadyRejected()
    {
        var visitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CancelVisitCommand(visitId, userId);
        var visit = new Visit
        {
            VisitId = visitId,
            UserId = userId,
            Status = VisitStatus.Rejected
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenVisitAlreadyApproved()
    {
        var visitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CancelVisitCommand(visitId, userId);
        var visit = new Visit
        {
            VisitId = visitId,
            UserId = userId,
            Status = VisitStatus.Approved
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenVisitAlreadyCompleted()
    {
        var visitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CancelVisitCommand(visitId, userId);
        var visit = new Visit
        {
            VisitId = visitId,
            UserId = userId,
            Status = VisitStatus.Completed
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_NotCallUpdate_WhenUserDoesNotOwnVisit()
    {
        var visitId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var command = new CancelVisitCommand(visitId, otherUserId);
        var visit = new Visit
        {
            VisitId = visitId,
            UserId = ownerId,
            Status = VisitStatus.Pending
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        await _handler.Handle(command, CancellationToken.None);

        VisitRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>()), Times.Never);
        UowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "11111111-1111-1111-1111-111111111111", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "00000000-0000-0000-0000-000000000000", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", true)]
    public async Task Validator_Should_ValidateCorrectly(string visitIdStr, string userIdStr, bool isValid)
    {
        var command = new CancelVisitCommand(Guid.Parse(visitIdStr), Guid.Parse(userIdStr));
        var validator = new CancelVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
    }
}
