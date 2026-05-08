namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Events;

public class VisitChangedEventHandlerTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_Should_InvalidateVisitsCache()
    {
        // Arrange
        var visit = await SeedVisitAsync();
        var handler = new VisitChangedEventHandler(HybridCache);

        // Fill cache
        await VisitRepository.GetWithTariffByIdAsync(visit.VisitId);

        // Modify DB directly
        var dbVisit = await Context.Visits.FindAsync(visit.VisitId);
        dbVisit!.Status = VisitStatus.Completed;
        await Context.SaveChangesAsync();

        // Act
        await handler.Handle(new VisitChangedEvent(visit.VisitId, visit.UserId));

        // Assert
        var result = await VisitRepository.GetWithTariffByIdAsync(visit.VisitId);
        result.Should().NotBeNull();
        result!.Status.Should().Be(VisitStatus.Completed); // Should be NEW status because cache was invalidated
    }
}
