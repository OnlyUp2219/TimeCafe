namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;

public class CreateVisitCommandTests : BaseCqrsHandlerTest
{
    private readonly CreateVisitCommandHandler _handler;

    public CreateVisitCommandTests()
    {
        _handler = new CreateVisitCommandHandler(VisitRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitCreated()
    {
        var command = new CreateVisitCommand("user123", 1);
        var visit = new Visit
        {
            VisitId = 1,
            UserId = "user123",
            TariffId = 1,
            EntryTime = DateTime.UtcNow,
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Visit>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Visit.Should().NotBeNull();
        result.Visit!.UserId.Should().Be("user123");
        result.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsNull()
    {
        var command = new CreateVisitCommand("user123", 1);

        VisitRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Visit>())).ReturnsAsync((Visit?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreateVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var command = new CreateVisitCommand("user123", 1);

        VisitRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Visit>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreateVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", 1, false, "ID пользователя обязателен")]
    [InlineData(null, 1, false, "ID пользователя обязателен")]
    [InlineData("a", 0, false, "ID тарифа обязателен")]
    [InlineData("a", -1, false, "ID тарифа обязателен")]
    [InlineData("user123", 1, true, null)]
    public async Task Validator_Should_ValidateCorrectly(string? userId, int tariffId, bool isValid, string? expectedError)
    {
        var command = new CreateVisitCommand(userId!, tariffId);
        var validator = new CreateVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
