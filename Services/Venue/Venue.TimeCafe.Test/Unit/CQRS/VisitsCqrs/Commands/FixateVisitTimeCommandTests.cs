using Microsoft.Extensions.Options;
using Venue.TimeCafe.Application.CQRS.Visits.Commands;
using Venue.TimeCafe.Application.Options;

namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Commands;

public class FixateVisitTimeCommandTests : BaseCqrsHandlerTest
{
    private readonly FixateVisitTimeCommandHandler _handler;

    public FixateVisitTimeCommandTests()
    {
        var optionsSnapshotMock = new Mock<IOptionsSnapshot<VenuePricingOptions>>();
        optionsSnapshotMock.Setup(o => o.Value).Returns(new VenuePricingOptions());

        _handler = new FixateVisitTimeCommandHandler(
            UowMock.Object,
            MapperMock.Object,
            PublishEndpointMock.Object,
            PublisherMock.Object,
            optionsSnapshotMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitTimeFixated()
    {
        var visitId = Guid.NewGuid();
        var command = new FixateVisitTimeCommand(visitId);
        
        var visitDto = new VisitWithTariffDto
        {
            VisitId = visitId,
            UserId = Guid.NewGuid(),
            TariffId = Guid.NewGuid(),
            EntryTime = DateTimeOffset.UtcNow.AddHours(-1),
            Status = VisitStatus.Active,
            TariffPricePerMinute = 2.5m,
            TariffBillingType = BillingType.PerMinute,
            TariffMinSessionMinutes = 0,
            TariffRoundingRule = "None"
        };

        var visit = new Visit { VisitId = visitId, EntryTime = DateTimeOffset.UtcNow.AddHours(-1), Status = VisitStatus.Active };

        VisitRepositoryMock.Setup(r => r.GetWithTariffByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visitDto);
        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);
        PromotionRepositoryMock.Setup(r => r.GetActiveByDateAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        UserLoyaltyRepositoryMock.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserLoyalty?)null);
        VisitRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>())).ReturnsAsync((Visit v, CancellationToken _) => v);
        UowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.CalculatedCost.Should().BeGreaterThan(0);
        visit.Status.Should().Be(VisitStatus.WaitingForPayment);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var visitId = Guid.NewGuid();
        var command = new FixateVisitTimeCommand(visitId);

        VisitRepositoryMock.Setup(r => r.GetWithTariffByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync((VisitWithTariffDto?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenVisitNotActive()
    {
        var visitId = Guid.NewGuid();
        var command = new FixateVisitTimeCommand(visitId);

        var visitDto = new VisitWithTariffDto
        {
            VisitId = visitId,
            UserId = Guid.NewGuid(),
            TariffId = Guid.NewGuid(),
            EntryTime = DateTimeOffset.UtcNow.AddHours(-1),
            Status = VisitStatus.Pending,
            TariffPricePerMinute = 2.5m,
            TariffRoundingRule = "None"
        };

        var visit = new Visit { VisitId = visitId, EntryTime = DateTimeOffset.UtcNow.AddHours(-1), Status = VisitStatus.Pending };

        VisitRepositoryMock.Setup(r => r.GetWithTariffByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visitDto);
        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }
}
