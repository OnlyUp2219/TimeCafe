namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

public class GetWithTariffByIdAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetWithTariffByIdAsync_Should_ReturnVisitWithTariff()
    {
        // Arrange
        var tariff = await SeedTariffAsync("Premium", 200m);
        var visit = await SeedVisitAsync(tariffId: tariff.TariffId);

        // Act
        var result = await VisitRepository.GetWithTariffByIdAsync(visit.VisitId);

        // Assert
        result.Should().NotBeNull();
        result!.VisitId.Should().Be(visit.VisitId);
        result.TariffName.Should().Be("Premium");
        result.TariffPricePerMinute.Should().Be(200m);
    }

    [Fact]
    public async Task Repository_GetWithTariffByIdAsync_Should_ReturnNull_WhenNotFound()
    {
        // Act
        var result = await VisitRepository.GetWithTariffByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetWithTariffByIdAsync_Should_ReturnFromCache_WhenCached()
    {
        // Arrange
        var tariff = await SeedTariffAsync("Premium", 200m);
        var visit = await SeedVisitAsync(tariffId: tariff.TariffId);

        // First call fills cache
        await VisitRepository.GetWithTariffByIdAsync(visit.VisitId);

        // Modify DB directly
        var dbVisit = await Context.Visits.FindAsync(visit.VisitId);
        dbVisit!.Status = VisitStatus.Completed;
        await Context.SaveChangesAsync();

        // Act
        var result = await VisitRepository.GetWithTariffByIdAsync(visit.VisitId);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(VisitStatus.Active); // Should be old status from cache
    }
}
