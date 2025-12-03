namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;

public class UpdateVisitCommandTests : BaseCqrsHandlerTest
{
    private readonly UpdateVisitCommandHandler _handler;

    public UpdateVisitCommandTests()
    {
        _handler = new UpdateVisitCommandHandler(VisitRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitUpdated()
    {
        var visit = new Visit
        {
            VisitId = 1,
            UserId = "user123",
            TariffId = 1,
            EntryTime = DateTime.UtcNow,
            Status = VisitStatus.Active
        };
        var command = new UpdateVisitCommand(visit);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(visit);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(visit)).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Visit.Should().NotBeNull();
        result.Visit!.UserId.Should().Be("user123");
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var visit = new Visit
        {
            VisitId = 999,
            UserId = "user123",
            TariffId = 1,
            EntryTime = DateTime.UtcNow,
            Status = VisitStatus.Active
        };
        var command = new UpdateVisitCommand(visit);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Visit?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("VisitNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsNull()
    {
        var visit = new Visit
        {
            VisitId = 1,
            UserId = "user123",
            TariffId = 1,
            EntryTime = DateTime.UtcNow,
            Status = VisitStatus.Active
        };
        var command = new UpdateVisitCommand(visit);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(visit);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(visit)).ReturnsAsync((Visit?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("UpdateVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var visit = new Visit
        {
            VisitId = 1,
            UserId = "user123",
            TariffId = 1,
            EntryTime = DateTime.UtcNow,
            Status = VisitStatus.Active
        };
        var command = new UpdateVisitCommand(visit);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("UpdateVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(null, "user123", 1, false, "Посещение обязательно")]
    [InlineData(0, "user123", 1, false, "ID посещения обязателен")]
    [InlineData(-1, "user123", 1, false, "ID посещения обязателен")]
    [InlineData(1, "", 1, false, "ID пользователя обязателен")]
    [InlineData(1, null, 1, false, "ID пользователя обязателен")]
    [InlineData(1, "user123", 0, false, "ID тарифа обязателен")]
    [InlineData(1, "user123", -1, false, "ID тарифа обязателен")]
    [InlineData(1, "user123", 1, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int? visitId, string? userId, int tariffId, bool isValid, string? expectedError)
    {
        var visit = visitId.HasValue ? new Visit
        {
            VisitId = visitId.Value,
            UserId = userId!,
            TariffId = tariffId,
            EntryTime = DateTime.UtcNow,
            Status = VisitStatus.Active
        } : null;
        var command = new UpdateVisitCommand(visit!);
        var validator = new UpdateVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
