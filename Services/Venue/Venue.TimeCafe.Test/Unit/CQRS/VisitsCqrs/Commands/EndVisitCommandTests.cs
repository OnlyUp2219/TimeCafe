namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;

public class EndVisitCommandTests : BaseCqrsHandlerTest
{
    private readonly EndVisitCommandHandler _handler;

    public EndVisitCommandTests()
    {
        _handler = new EndVisitCommandHandler(VisitRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitEnded()
    {
        var command = new EndVisitCommand(1);
        var visit = new Visit
        {
            VisitId = 1,
            UserId = "user123",
            TariffId = 1,
            EntryTime = DateTime.UtcNow.AddHours(-1),
            Status = VisitStatus.Active,
            Tariff = new Tariff
            {
                TariffId = 1,
                PricePerMinute = 1.5m,
                BillingType = BillingType.PerMinute
            }
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(visit);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Visit>())).ReturnsAsync((Visit v) => v);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Visit.Should().NotBeNull();
        result.Visit!.Status.Should().Be(VisitStatus.Completed);
        result.CalculatedCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var command = new EndVisitCommand(999);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Visit?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("VisitNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsNull()
    {
        var command = new EndVisitCommand(1);
        var visit = new Visit
        {
            VisitId = 1,
            UserId = "user123",
            TariffId = 1,
            EntryTime = DateTime.UtcNow.AddHours(-1),
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(visit);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Visit>())).ReturnsAsync((Visit?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("EndVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var command = new EndVisitCommand(1);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("EndVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(0, false, "ID посещения обязателен")]
    [InlineData(-1, false, "ID посещения обязателен")]
    [InlineData(1, true, null)]
    [InlineData(999, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int visitId, bool isValid, string? expectedError)
    {
        var command = new EndVisitCommand(visitId);
        var validator = new EndVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
