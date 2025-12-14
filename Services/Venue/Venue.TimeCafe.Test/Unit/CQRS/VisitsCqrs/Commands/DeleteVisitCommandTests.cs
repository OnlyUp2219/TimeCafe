namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;

public class DeleteVisitCommandTests : BaseCqrsHandlerTest
{
    private readonly DeleteVisitCommandHandler _handler;

    public DeleteVisitCommandTests()
    {
        _handler = new DeleteVisitCommandHandler(VisitRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitDeleted()
    {
        var command = new DeleteVisitCommand(1);
        var visit = new Visit
        {
            VisitId = 1,
            UserId = "user123",
            TariffId = 1,
            EntryTime = DateTime.UtcNow,
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(visit);
        VisitRepositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var command = new DeleteVisitCommand(999);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Visit?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("VisitNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsFalse()
    {
        var command = new DeleteVisitCommand(1);
        var visit = new Visit
        {
            VisitId = 1,
            UserId = "user123",
            TariffId = 1,
            EntryTime = DateTime.UtcNow,
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(visit);
        VisitRepositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeleteVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var command = new DeleteVisitCommand(1);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeleteVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(0, false, "ID посещения обязателен")]
    [InlineData(-1, false, "ID посещения обязателен")]
    [InlineData(1, true, null)]
    [InlineData(999, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int visitId, bool isValid, string? expectedError)
    {
        var command = new DeleteVisitCommand(visitId);
        var validator = new DeleteVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
