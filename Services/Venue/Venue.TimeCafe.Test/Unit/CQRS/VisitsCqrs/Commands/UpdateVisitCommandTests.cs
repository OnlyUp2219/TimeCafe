namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;



public class UpdateVisitCommandTests : BaseCqrsHandlerTest
{
    private readonly UpdateVisitCommandHandler _handler;

    public UpdateVisitCommandTests()
    {
        _handler = new UpdateVisitCommandHandler(VisitRepositoryMock.Object, TariffRepositoryMock.Object, MapperMock.Object);

        MapperMock.Setup(m => m.Map(It.IsAny<UpdateVisitCommand>(), It.IsAny<VisitWithTariffDto>()))
            .Callback((UpdateVisitCommand cmd, VisitWithTariffDto dto) =>
            {
                dto.UserId = Guid.Parse(cmd.UserId);
                dto.TariffId = Guid.Parse(cmd.TariffId);
                dto.EntryTime = cmd.EntryTime;
                dto.ExitTime = cmd.ExitTime;
                dto.CalculatedCost = cmd.CalculatedCost;
                dto.Status = cmd.Status;
            });

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
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitUpdated()
    {
        var visitId = Guid.NewGuid();
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = TestData.DefaultValues.DefaultTariffId;
        var entryTime = DateTimeOffset.UtcNow;

        var command = new UpdateVisitCommand(
            visitId.ToString(),
            userId.ToString(),
            tariffId.ToString(),
            entryTime,
            null,
            null,
            VisitStatus.Active);

        var existingVisitDto = new VisitWithTariffDto
        {
            VisitId = visitId,
            UserId = userId,
            TariffId = tariffId,
            EntryTime = entryTime,
            Status = VisitStatus.Active
        };

        var visit = new Visit
        {
            VisitId = visitId,
            UserId = userId,
            TariffId = tariffId,
            EntryTime = entryTime,
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId)).ReturnsAsync(existingVisitDto);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Visit>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Visit.Should().NotBeNull();
        result.Visit!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var visitId = TestData.NonExistingIds.NonExistingVisitId;
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = TestData.DefaultValues.DefaultTariffId;

        var command = new UpdateVisitCommand(
            visitId.ToString(),
            userId.ToString(),
            tariffId.ToString(),
            DateTimeOffset.UtcNow,
            null,
            null,
            VisitStatus.Active);

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
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = TestData.DefaultValues.DefaultTariffId;
        var entryTime = DateTimeOffset.UtcNow;

        var command = new UpdateVisitCommand(
            visitId.ToString(),
            userId.ToString(),
            tariffId.ToString(),
            entryTime,
            null,
            null,
            VisitStatus.Active);

        var existingVisitDto = new VisitWithTariffDto
        {
            VisitId = visitId,
            UserId = userId,
            TariffId = tariffId,
            EntryTime = entryTime,
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId)).ReturnsAsync(existingVisitDto);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Visit>())).ReturnsAsync((Visit?)null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("UpdateVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ThrowCqrsResultException_WhenExceptionThrown()
    {
        var visitId = Guid.NewGuid();
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = TestData.NonExistingIds.NonExistingTariffId;

        var command = new UpdateVisitCommand(
            visitId.ToString(),
            userId.ToString(),
            tariffId.ToString(),
            DateTimeOffset.UtcNow,
            null,
            null,
            VisitStatus.Active);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId)).ThrowsAsync(new Exception());

        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("UpdateVisitFailed");
        ex.Result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnTariffNotFound_WhenTariffDoesNotExist()
    {
        var visitId = Guid.NewGuid();
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = Guid.NewGuid();
        var entryTime = DateTimeOffset.UtcNow;

        var existingDto = new VisitWithTariffDto
        {
            VisitId = visitId,
            UserId = userId,
            TariffId = tariffId,
            EntryTime = entryTime,
            Status = VisitStatus.Active
        };

        var command = new UpdateVisitCommand(
            visitId.ToString(),
            userId.ToString(),
            tariffId.ToString(),
            entryTime,
            null,
            null,
            VisitStatus.Active);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId)).ReturnsAsync(existingDto);
        TariffRepositoryMock.Setup(r => r.GetByIdAsync(tariffId)).ReturnsAsync((TariffWithThemeDto?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("TariffNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Theory]
    [InlineData("", "11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "", "22222222-2222-2222-2222-222222222222", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", "", false)]
    [InlineData("not-a-guid", "11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", "33333333-3333-3333-3333-333333333333", true)]
    public async Task Validator_Should_ValidateCorrectly(string visitId, string userId, string tariffId, bool isValid)
    {
        var command = new UpdateVisitCommand(
            visitId,
            userId,
            tariffId,
            DateTimeOffset.UtcNow,
            null,
            null,
            VisitStatus.Active);
        var validator = new UpdateVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
    }
}
