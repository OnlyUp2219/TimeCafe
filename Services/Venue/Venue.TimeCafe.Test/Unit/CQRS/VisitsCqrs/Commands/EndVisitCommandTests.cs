using Microsoft.Extensions.Options;
using Venue.TimeCafe.Application.Options;

namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;

public class EndVisitCommandTests : BaseCqrsHandlerTest
{
    private readonly EndVisitCommandHandler _handler;

    public EndVisitCommandTests()
    {
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<EndVisitCommandHandler>>();
        var optionsSnapshotMock = new Mock<IOptionsSnapshot<VenuePricingOptions>>();
        optionsSnapshotMock.Setup(o => o.Value).Returns(new VenuePricingOptions());

        _handler = new EndVisitCommandHandler(
            UowMock.Object,
            MapperMock.Object,
            PublishEndpointMock.Object,
            PublisherMock.Object,
            loggerMock.Object,
            optionsSnapshotMock.Object);

        MapperMock.Setup(m => m.Map<Visit>(It.IsAny<VisitWithTariffDto>()))
            .Returns((VisitWithTariffDto src) => new Visit(src.VisitId)
            {
                UserId = src.UserId,
                TariffId = src.TariffId,
                EntryTime = src.EntryTime,
                ExitTime = src.ExitTime,
                CalculatedCost = src.CalculatedCost,
                Status = src.Status
            });

        MapperMock.Setup(m => m.Map(It.IsAny<EndVisitCommand>(), It.IsAny<Visit>()))
            .Callback((EndVisitCommand _, Visit _) =>
            {
            });
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitEnded()
    {
        var visitId = Guid.NewGuid();
        var command = new EndVisitCommand(visitId);
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

        var visit = new Visit { VisitId = visitId, EntryTime = DateTimeOffset.UtcNow.AddHours(-1), Status = VisitStatus.Active };
        VisitRepositoryMock.Setup(r => r.GetWithTariffByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visitDto);
        PromotionRepositoryMock.Setup(r => r.GetActiveByDateAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        UserLoyaltyRepositoryMock.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserLoyalty?)null);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>())).ReturnsAsync((Visit v, CancellationToken _) => v);
        UowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CalculatedCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var visitId = TestData.NonExistingIds.NonExistingVisitId;
        var command = new EndVisitCommand(visitId);

        VisitRepositoryMock.Setup(r => r.GetWithTariffByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync((VisitWithTariffDto?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsNull()
    {
        var visitId = Guid.NewGuid();
        var command = new EndVisitCommand(visitId);
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

        var visit = new Visit { VisitId = visitId, EntryTime = DateTimeOffset.UtcNow.AddHours(-1), Status = VisitStatus.Active };
        VisitRepositoryMock.Setup(r => r.GetWithTariffByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visitDto);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>())).ReturnsAsync((Visit?)null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var visitId = Guid.NewGuid();
        var command = new EndVisitCommand(visitId);

        VisitRepositoryMock.Setup(r => r.GetWithTariffByIdAsync(visitId, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", true)]
    public async Task Validator_Should_ValidateCorrectly(string visitIdStr, bool isValid)
    {
        var command = new EndVisitCommand(Guid.Parse(visitIdStr));
        var validator = new EndVisitCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
    }
}

