namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;

public class EndVisitCommandTests : BaseCqrsHandlerTest
{
    private readonly EndVisitCommandHandler _handler;

    public EndVisitCommandTests()
    {
        _handler = new EndVisitCommandHandler(VisitRepositoryMock.Object, MapperMock.Object, PublishEndpointMock.Object);

        MapperMock.Setup(m => m.Map<Visit>(It.IsAny<VisitWithTariffDto>()))
            .Returns((VisitWithTariffDto dto) => new Visit(dto.VisitId)
            {
                UserId = dto.UserId,
                TariffId = dto.TariffId,
                EntryTime = dto.EntryTime,
                ExitTime = dto.ExitTime,
                CalculatedCost = dto.CalculatedCost,
                Status = dto.Status
            });

        MapperMock.Setup(m => m.Map(It.IsAny<EndVisitCommand>(), It.IsAny<Visit>()))
            .Callback((EndVisitCommand cmd, Visit v) =>
            {
            });
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitEnded()
    {
        var visitId = Guid.NewGuid();
        var command = new EndVisitCommand(visitId.ToString());
        var visitDto = new VisitWithTariffDto
        {
            VisitId = visitId,
            UserId = TestData.ExistingVisits.Visit1UserId,
            TariffId = Guid.NewGuid(),
            EntryTime = DateTimeOffset.UtcNow.AddHours(-1),
            Status = VisitStatus.Active,
            TariffPricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            TariffBillingType = TestData.ExistingTariffs.Tariff1BillingType
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId)).ReturnsAsync(visitDto);
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
        var visitId = TestData.NonExistingIds.NonExistingVisitId;
        var command = new EndVisitCommand(visitId.ToString());

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId)).ReturnsAsync((VisitWithTariffDto?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("VisitNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsNull()
    {
        var visitId = Guid.NewGuid();
        var command = new EndVisitCommand(visitId.ToString());
        var visitDto = new VisitWithTariffDto
        {
            VisitId = visitId,
            UserId = TestData.ExistingVisits.Visit1UserId,
            TariffId = Guid.NewGuid(),
            EntryTime = DateTimeOffset.UtcNow.AddHours(-1),
            Status = VisitStatus.Active,
            TariffPricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            TariffBillingType = TestData.ExistingTariffs.Tariff1BillingType
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId)).ReturnsAsync(visitDto);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Visit>())).ReturnsAsync((Visit?)null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("EndVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ThrowCqrsResultException_WhenExceptionThrown()
    {
        var visitId = Guid.NewGuid();
        var command = new EndVisitCommand(visitId.ToString());

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId)).ThrowsAsync(new Exception());

        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("EndVisitFailed");
        ex.Result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("not-a-guid", false)]
    [InlineData("00000000-0000-0000-0000-000000000000", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", true)]
    public async Task Validator_Should_ValidateCorrectly(string visitId, bool isValid)
    {
        var command = new EndVisitCommand(visitId);
        var validator = new EndVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
    }
}
