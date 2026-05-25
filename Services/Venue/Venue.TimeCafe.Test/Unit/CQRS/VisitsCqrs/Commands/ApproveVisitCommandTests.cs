namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;

public class ApproveVisitCommandTests : BaseCqrsHandlerTest
{
    private readonly ApproveVisitCommandHandler _handler;

    public ApproveVisitCommandTests()
    {
        _handler = new ApproveVisitCommandHandler(UowMock.Object, PublishEndpointMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitApproved()
    {
        var visitId = Guid.NewGuid();
        var approvedByUserId = Guid.NewGuid();
        var command = new ApproveVisitCommand(visitId, approvedByUserId);
        var visit = new Visit
        {
            VisitId = visitId,
            UserId = Guid.NewGuid(),
            TariffId = Guid.NewGuid(),
            Status = VisitStatus.Pending
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>())).ReturnsAsync((Visit v, CancellationToken _) => v);
        UowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(VisitStatus.Active);
        result.Value.ApprovedByUserId.Should().Be(approvedByUserId);
        result.Value.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var visitId = TestData.NonExistingIds.NonExistingVisitId;
        var command = new ApproveVisitCommand(visitId, Guid.NewGuid());

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync((Visit?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenVisitNotPending()
    {
        var visitId = Guid.NewGuid();
        var command = new ApproveVisitCommand(visitId, Guid.NewGuid());
        var visit = new Visit
        {
            VisitId = visitId,
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
        var command = new ApproveVisitCommand(visitId, Guid.NewGuid());
        var visit = new Visit
        {
            VisitId = visitId,
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
        var command = new ApproveVisitCommand(visitId, Guid.NewGuid());

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenVisitAlreadyApproved()
    {
        var visitId = Guid.NewGuid();
        var command = new ApproveVisitCommand(visitId, Guid.NewGuid());
        var visit = new Visit
        {
            VisitId = visitId,
            Status = VisitStatus.Approved,
            ApprovedByUserId = Guid.NewGuid(),
            ApprovedAt = DateTimeOffset.UtcNow
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenVisitAlreadyRejected()
    {
        var visitId = Guid.NewGuid();
        var command = new ApproveVisitCommand(visitId, Guid.NewGuid());
        var visit = new Visit
        {
            VisitId = visitId,
            Status = VisitStatus.Rejected,
            RejectionReason = "Нет мест"
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenVisitAlreadyCancelled()
    {
        var visitId = Guid.NewGuid();
        var command = new ApproveVisitCommand(visitId, Guid.NewGuid());
        var visit = new Visit
        {
            VisitId = visitId,
            Status = VisitStatus.Cancelled
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenVisitAlreadyCompleted()
    {
        var visitId = Guid.NewGuid();
        var command = new ApproveVisitCommand(visitId, Guid.NewGuid());
        var visit = new Visit
        {
            VisitId = visitId,
            Status = VisitStatus.Completed
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_PublishEvent_WhenVisitApproved()
    {
        var visitId = Guid.NewGuid();
        var approvedByUserId = Guid.NewGuid();
        var command = new ApproveVisitCommand(visitId, approvedByUserId);
        var visit = new Visit
        {
            VisitId = visitId,
            UserId = Guid.NewGuid(),
            TariffId = Guid.NewGuid(),
            Status = VisitStatus.Pending
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>())).ReturnsAsync((Visit v, CancellationToken _) => v);
        UowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        PublishEndpointMock.Verify(p => p.Publish(It.Is<VisitApprovedEvent>(e => e.VisitId == visitId && e.ApprovedByUserId == approvedByUserId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "11111111-1111-1111-1111-111111111111", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "00000000-0000-0000-0000-000000000000", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", true)]
    public async Task Validator_Should_ValidateCorrectly(string visitIdStr, string approvedByUserIdStr, bool isValid)
    {
        var command = new ApproveVisitCommand(Guid.Parse(visitIdStr), Guid.Parse(approvedByUserIdStr));
        var validator = new ApproveVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
    }
}
