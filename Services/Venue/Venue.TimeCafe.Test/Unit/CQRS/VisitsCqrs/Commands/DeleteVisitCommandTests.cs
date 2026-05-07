namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;

public class DeleteVisitCommandTests : BaseCqrsHandlerTest
{
    private readonly DeleteVisitCommandHandler _handler;

    public DeleteVisitCommandTests()
    {
        _handler = new DeleteVisitCommandHandler(UowMock.Object, PublisherMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitDeleted()
    {
        var visitId = TestData.ExistingVisits.Visit1UserId;
        var command = new DeleteVisitCommand(visitId);
        var visit = new Visit { VisitId = visitId };
        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);
        VisitRepositoryMock.Setup(r => r.DeleteAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        UowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var visitId = TestData.NonExistingIds.NonExistingVisitId;
        var command = new DeleteVisitCommand(visitId);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync((Visit?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsFalse()
    {
        var visitId = TestData.ExistingVisits.Visit1UserId;
        var command = new DeleteVisitCommand(visitId);
        var visit = new Visit { VisitId = visitId };
        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);
        VisitRepositoryMock.Setup(r => r.DeleteAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var visitId = TestData.ExistingVisits.Visit1UserId;
        var command = new DeleteVisitCommand(visitId);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", false, "Посещение не найдено")]
    [InlineData("11111111-1111-1111-1111-111111111111", true, null)]
    public async Task Validator_Should_ValidateCorrectly(string visitIdStr, bool isValid, string? expectedError)
    {
        var command = new DeleteVisitCommand(Guid.Parse(visitIdStr));
        var validator = new DeleteVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}

