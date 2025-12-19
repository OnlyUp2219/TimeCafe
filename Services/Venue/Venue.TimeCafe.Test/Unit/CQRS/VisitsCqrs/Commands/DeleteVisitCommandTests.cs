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
        var visitId = TestData.ExistingVisits.Visit1UserId;
        var command = new DeleteVisitCommand(visitId.ToString());
        var visitDto = new VisitWithTariffDto
        {
            VisitId = visitId,
            UserId = TestData.ExistingVisits.Visit1UserId,
            TariffId = Guid.NewGuid(),
            EntryTime = DateTimeOffset.UtcNow,
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId)).ReturnsAsync(visitDto);
        VisitRepositoryMock.Setup(r => r.DeleteAsync(visitId)).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var visitId = TestData.NonExistingIds.NonExistingVisitId;
        var command = new DeleteVisitCommand(visitId.ToString());

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId)).ReturnsAsync((VisitWithTariffDto?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("VisitNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsFalse()
    {
        var visitId = TestData.ExistingVisits.Visit1UserId;
        var command = new DeleteVisitCommand(visitId.ToString());
        var visitDto = new VisitWithTariffDto
        {
            VisitId = visitId,
            UserId = TestData.ExistingVisits.Visit1UserId,
            TariffId = Guid.NewGuid(),
            EntryTime = DateTimeOffset.UtcNow,
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId)).ReturnsAsync(visitDto);
        VisitRepositoryMock.Setup(r => r.DeleteAsync(visitId)).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeleteVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var visitId = TestData.ExistingVisits.Visit1UserId;
        var command = new DeleteVisitCommand(visitId.ToString());

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeleteVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false, "Посещение не найдено")]
    [InlineData("not-a-guid", false, "Посещение не найдено")]
    [InlineData("00000000-0000-0000-0000-000000000000", false, "Посещение не найдено")]
    [InlineData("11111111-1111-1111-1111-111111111111", true, null)]
    public async Task Validator_Should_ValidateCorrectly(string visitId, bool isValid, string? expectedError)
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
