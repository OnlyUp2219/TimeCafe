namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;

public class CreateVisitCommandTests : BaseCqrsHandlerTest
{
    private readonly CreateVisitCommandHandler _handler;

    public CreateVisitCommandTests()
    {
        _handler = new CreateVisitCommandHandler(
            VisitRepositoryMock.Object,
            TariffRepositoryMock.Object,
            VisitBalancePolicyServiceMock.Object);
        TariffRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new TariffWithThemeDto
            {
                TariffId = TestData.DefaultValues.DefaultTariffId,
                Name = TestData.DefaultValues.DefaultTariffName,
                PricePerMinute = TestData.DefaultValues.DefaultTariffPrice,
                BillingType = TestData.DefaultValues.DefaultBillingType,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                LastModified = DateTimeOffset.UtcNow
            });
        VisitBalancePolicyServiceMock
            .Setup(x => x.CheckBeforeStartAsync(
                It.IsAny<Guid>(),
                It.IsAny<TariffWithThemeDto>(),
                It.IsAny<int?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VisitBalanceCheckResult(true));
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitCreated()
    {
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = TestData.DefaultValues.DefaultTariffId;
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
        var tariffId = TestData.DefaultValues.DefaultTariffId;
        var command = new CreateVisitCommand(userId.ToString(), tariffId.ToString());

        VisitRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Visit>())).ReturnsAsync((Visit?)null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreateVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ThrowCqrsResultException_WhenExceptionThrown()
    {
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = TestData.DefaultValues.DefaultTariffId;
        var command = new CreateVisitCommand(userId.ToString(), tariffId.ToString());

        VisitRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Visit>())).ThrowsAsync(new Exception());

        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("CreateVisitFailed");
        ex.Result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnTariffNotFound_WhenTariffDoesNotExist()
    {
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = TestData.NonExistingIds.NonExistingTariffId;
        var command = new CreateVisitCommand(userId.ToString(), tariffId.ToString());

        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ReturnsAsync((TariffWithThemeDto?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("TariffNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnInsufficientFunds_WhenBalancePolicyDenied()
    {
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = TestData.DefaultValues.DefaultTariffId;
        var command = new CreateVisitCommand(userId.ToString(), tariffId.ToString(), 120, true, true);

        VisitBalancePolicyServiceMock
            .Setup(x => x.CheckBeforeStartAsync(
                userId,
                It.IsAny<TariffWithThemeDto>(),
                120,
                true,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VisitBalanceCheckResult(false, "Недостаточно средств"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("InsufficientFunds");
        result.StatusCode.Should().Be(400);
        VisitRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Visit>()), Times.Never);
    }

    [Fact]
    public async Task Handler_Should_ReturnBalanceCheckFailed_WhenBalanceServiceTimeout()
    {
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = TestData.DefaultValues.DefaultTariffId;
        var command = new CreateVisitCommand(userId.ToString(), tariffId.ToString(), 90, true, true);

        VisitBalancePolicyServiceMock
            .Setup(x => x.CheckBeforeStartAsync(
                It.IsAny<Guid>(),
                It.IsAny<TariffWithThemeDto>(),
                It.IsAny<int?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("timeout"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("BalanceCheckFailed");
        result.StatusCode.Should().Be(503);
    }

    [Theory]
    [InlineData("", "11111111-1111-1111-1111-111111111111", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "", false)]
    [InlineData("not-a-guid", "11111111-1111-1111-1111-111111111111", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "not-a-guid", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", true, 30)]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", false, 0)]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", true)]
    public async Task Validator_Should_ValidateCorrectly(string? userId, string? tariffId, bool isValid, int? plannedMinutes = null)
    {
        var command = new CreateVisitCommand(userId!, tariffId!, plannedMinutes, true, plannedMinutes.HasValue);
        var validator = new CreateVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
    }
}
