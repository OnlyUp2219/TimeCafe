namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;

public class RejectVisitCommandTests : BaseCqrsHandlerTest
{
    private readonly RejectVisitCommandHandler _handler;

    public RejectVisitCommandTests()
    {
        _handler = new RejectVisitCommandHandler(UowMock.Object, PublishEndpointMock.Object, PublisherMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitRejected()
    {
        var visitId = Guid.NewGuid();
        var command = new RejectVisitCommand(visitId, "Нет свободных мест");
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
        result.Value!.Status.Should().Be(VisitStatus.Rejected);
        result.Value.RejectionReason.Should().Be("Нет свободных мест");
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var visitId = TestData.NonExistingIds.NonExistingVisitId;
        var command = new RejectVisitCommand(visitId, "Причина");

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync((Visit?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenVisitNotPending()
    {
        var visitId = Guid.NewGuid();
        var command = new RejectVisitCommand(visitId, "Причина");
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
        var command = new RejectVisitCommand(visitId, "Причина");
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
        var command = new RejectVisitCommand(visitId, "Причина");

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenVisitAlreadyRejected()
    {
        var visitId = Guid.NewGuid();
        var command = new RejectVisitCommand(visitId, "Другая причина");
        var visit = new Visit
        {
            VisitId = visitId,
            Status = VisitStatus.Rejected,
            RejectionReason = "Первая причина"
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenVisitAlreadyCancelled()
    {
        var visitId = Guid.NewGuid();
        var command = new RejectVisitCommand(visitId, "Причина");
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
        var command = new RejectVisitCommand(visitId, "Причина");
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
    public async Task Handler_Should_ReturnFailed_WhenVisitAlreadyApproved()
    {
        var visitId = Guid.NewGuid();
        var command = new RejectVisitCommand(visitId, "Причина");
        var visit = new Visit
        {
            VisitId = visitId,
            Status = VisitStatus.Approved
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_PublishEvent_WhenVisitRejected()
    {
        var visitId = Guid.NewGuid();
        var command = new RejectVisitCommand(visitId, "Нет мест");
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

        PublishEndpointMock.Verify(p => p.Publish(It.Is<VisitRejectedEvent>(e => e.VisitId == visitId && e.Reason == "Нет мест"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "Причина", true)]
    public async Task Validator_Should_ValidateCorrectly(string visitIdStr, string reason, bool isValid)
    {
        var command = new RejectVisitCommand(Guid.Parse(visitIdStr), reason);
        var validator = new RejectVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
    }

    [Theory]
    [InlineData(500, true)]
    [InlineData(501, false)]
    public async Task Validator_Should_EnforceReasonMaxLength(int length, bool isValid)
    {
        var reason = new string('x', length);
        var command = new RejectVisitCommand(Guid.NewGuid(), reason);
        var validator = new RejectVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
    }

    [Fact]
    public async Task Validator_Should_RejectWhitespaceOnlyReason()
    {
        var command = new RejectVisitCommand(Guid.NewGuid(), "   ");
        var validator = new RejectVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validator_Should_AcceptEmojiReason()
    {
        var command = new RejectVisitCommand(Guid.NewGuid(), "❌ Нет мест 😢");
        var validator = new RejectVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }
}
