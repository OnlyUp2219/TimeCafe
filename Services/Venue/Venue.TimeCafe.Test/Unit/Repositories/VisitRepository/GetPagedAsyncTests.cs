namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

public class GetPagedAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetPagedAsync_Should_ReturnPagedVisits()
    {
        // Arrange
        var tariff = await SeedTariffAsync();
        await SeedVisitAsync(tariffId: tariff.TariffId);
        await Task.Delay(10);
        await SeedVisitAsync(tariffId: tariff.TariffId);
        await Task.Delay(10);
        var visit3 = await SeedVisitAsync(tariffId: tariff.TariffId);

        // Act
        var result = (await VisitRepository.GetPagedAsync(1, 2)).ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        // Ordered by EntryTime descending, so visit3 should be first
        result[0].VisitId.Should().Be(visit3.VisitId);
    }

    [Fact]
    public async Task Repository_GetPagedAsync_Should_ReturnEmpty_WhenPageOutOfBounds()
    {
        // Arrange
        await SeedVisitAsync();

        // Act
        var result = await VisitRepository.GetPagedAsync(2, 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetPagedAsync_Should_ReturnFromCache_WhenCached()
    {
        // Arrange
        var visit = await SeedVisitAsync();

        // First call fills cache
        await VisitRepository.GetPagedAsync(1, 10);

        // Modify DB directly
        var dbVisit = await Context.Visits.FindAsync(visit.VisitId);
        dbVisit!.Status = VisitStatus.Completed;
        await Context.SaveChangesAsync();

        // Act
        var result = (await VisitRepository.GetPagedAsync(1, 10)).ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Status.Should().Be(VisitStatus.Active); // Should be old status from cache
    }
}
