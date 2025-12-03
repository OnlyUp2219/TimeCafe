namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

public class GetByIdAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnVisit_WhenExists()
    {
        // Arrange
        var visit = await SeedVisitAsync("user123");

        // Act
        var result = await VisitRepository.GetByIdAsync(visit.VisitId);

        // Assert
        result.Should().NotBeNull();
        result!.VisitId.Should().Be(visit.VisitId);
        result.UserId.Should().Be("user123");
        result.Status.Should().Be(VisitStatus.Active);
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await VisitRepository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        var visit = await SeedVisitAsync("user123");
        await VisitRepository.GetByIdAsync(visit.VisitId);
        await VisitRepository.GetByIdAsync(visit.VisitId);

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_IncludeTariffRelation()
    {
        // Arrange
        var tariff = await SeedTariffAsync("Test Tariff", 100m);
        var visit = await SeedVisitAsync("user123", tariff.TariffId);

        // Act
        var result = await VisitRepository.GetByIdAsync(visit.VisitId);

        // Assert
        result.Should().NotBeNull();
        result!.Tariff.Should().NotBeNull();
        result.Tariff.Name.Should().Be("Test Tariff");
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnVisitWithAllProperties()
    {
        // Arrange
        var tariff = await SeedTariffAsync("Premium", 200m);
        var visit = new Visit
        {
            UserId = "user456",
            TariffId = tariff.TariffId,
            EntryTime = DateTime.UtcNow,
            Status = VisitStatus.Active
        };
        Context.Visits.Add(visit);
        await Context.SaveChangesAsync();

        // Act
        var result = await VisitRepository.GetByIdAsync(visit.VisitId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be("user456");
        result.TariffId.Should().Be(tariff.TariffId);
        result.Status.Should().Be(VisitStatus.Active);
        result.EntryTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
