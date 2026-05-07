namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;



public class UpdateVisitCommandTests : BaseCqrsHandlerTest
{
    private readonly UpdateVisitCommandHandler _handler;

    public UpdateVisitCommandTests()
    {
        _handler = new UpdateVisitCommandHandler(UowMock.Object, MapperMock.Object, PublisherMock.Object);

        MapperMock.Setup(m => m.Map(It.IsAny<UpdateVisitCommand>(), It.IsAny<Visit>()))
            .Callback((UpdateVisitCommand cmd, Visit entity) =>
            {
                entity.UserId = cmd.UserId;
                entity.TariffId = cmd.TariffId;
                entity.EntryTime = cmd.EntryTime;
                entity.ExitTime = cmd.ExitTime;
                entity.CalculatedCost = cmd.CalculatedCost;
                entity.Status = cmd.Status;
            });

        MapperMock.Setup(m => m.Map<Visit>(It.IsAny<Visit>()))
            .Returns((Visit src) => new Visit(src.VisitId)
            {
                UserId = src.UserId,
                TariffId = src.TariffId,
                EntryTime = src.EntryTime,
                ExitTime = src.ExitTime,
                CalculatedCost = src.CalculatedCost,
                Status = src.Status
            });
        TariffRepositoryMock.Setup(r => r.GetWithThemeByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
            visitId,
            userId,
            tariffId,
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

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>())).ReturnsAsync(visit);
        UowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var visitId = TestData.NonExistingIds.NonExistingVisitId;
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = TestData.DefaultValues.DefaultTariffId;

        var command = new UpdateVisitCommand(
            visitId,
            userId,
            tariffId,
            DateTimeOffset.UtcNow,
            null,
            null,
            VisitStatus.Active);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync((Visit?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsNull()
    {
        var visitId = Guid.NewGuid();
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = TestData.DefaultValues.DefaultTariffId;
        var entryTime = DateTimeOffset.UtcNow;

        var command = new UpdateVisitCommand(
            visitId,
            userId,
            tariffId,
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

        var visit = new Visit { VisitId = visitId, UserId = userId, TariffId = tariffId, EntryTime = entryTime, Status = VisitStatus.Active };
        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>())).ReturnsAsync((Visit?)null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var visitId = Guid.NewGuid();
        var userId = TestData.ExistingVisits.Visit1UserId;
        var tariffId = TestData.NonExistingIds.NonExistingTariffId;

        var command = new UpdateVisitCommand(
            visitId,
            userId,
            tariffId,
            DateTimeOffset.UtcNow,
            null,
            null,
            VisitStatus.Active);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
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
            visitId,
            userId,
            tariffId,
            entryTime,
            null,
            null,
            VisitStatus.Active);

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(new Visit { VisitId = visitId });
        TariffRepositoryMock.Setup(r => r.GetWithThemeByIdAsync(tariffId, It.IsAny<CancellationToken>())).ReturnsAsync((TariffWithThemeDto?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "00000000-0000-0000-0000-000000000000", "22222222-2222-2222-2222-222222222222", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", "00000000-0000-0000-0000-000000000000", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", "33333333-3333-3333-3333-333333333333", true)]
    public async Task Validator_Should_ValidateCorrectly(string visitIdStr, string userIdStr, string tariffIdStr, bool isValid)
    {
        var command = new UpdateVisitCommand(
            Guid.Parse(visitIdStr),
            Guid.Parse(userIdStr),
            Guid.Parse(tariffIdStr),
            DateTimeOffset.UtcNow,
            null,
            null,
            VisitStatus.Active);
        var validator = new UpdateVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
    }
}

