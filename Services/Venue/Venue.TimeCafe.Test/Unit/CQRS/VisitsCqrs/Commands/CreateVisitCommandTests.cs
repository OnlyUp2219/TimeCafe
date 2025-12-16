namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;

using Venue.TimeCafe.Test.Integration.Helpers;

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
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = Guid.NewGuid();
        var command = new CreateVisitCommand(userId.ToString(), tariffId.ToString());
        var visit = new Visit
        {
            VisitId = Guid.NewGuid(),
            UserId = userId,
            TariffId = tariffId,
            EntryTime = DateTimeOffset.UtcNow,
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Visit>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Visit.Should().NotBeNull();
        result.Visit!.UserId.Should().Be(userId);
        result.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsNull()
    {
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = Guid.NewGuid();
        var command = new CreateVisitCommand(userId.ToString(), tariffId.ToString());

        VisitRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Visit>())).ReturnsAsync((Visit?)null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreateVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = Guid.NewGuid();
        var command = new CreateVisitCommand(userId.ToString(), tariffId.ToString());

        VisitRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Visit>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreateVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", "11111111-1111-1111-1111-111111111111", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "", false)]
    [InlineData("not-a-guid", "11111111-1111-1111-1111-111111111111", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "not-a-guid", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", true)]
    public async Task Validator_Should_ValidateCorrectly(string? userId, string? tariffId, bool isValid)
    {
        var command = new CreateVisitCommand(userId!, tariffId!);
        var validator = new CreateVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
    }
}
