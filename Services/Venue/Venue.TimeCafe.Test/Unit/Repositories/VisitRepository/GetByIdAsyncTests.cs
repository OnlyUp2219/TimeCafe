namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

public class GetByIdAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnVisit_WhenExists()
    {
        // Arrange
        var visit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);

        // Act
        var result = await VisitRepository.GetByIdAsync(visit.VisitId);

        // Assert
        result.Should().NotBeNull();
        result!.VisitId.Should().Be(visit.VisitId);
        result.UserId.Should().Be(TestData.ExistingVisits.Visit1UserId);
        result.Status.Should().Be(VisitStatus.Active);
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = TestData.NonExistingIds.NonExistingVisitId;

        // Act
        var result = await VisitRepository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        var visit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        await VisitRepository.GetByIdAsync(visit.VisitId);
        await VisitRepository.GetByIdAsync(visit.VisitId);

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_IncludeTariffRelation()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);
        var visit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId, tariff.TariffId);

        // Act
        var result = await VisitRepository.GetByIdAsync(visit.VisitId);

        // Assert
        result.Should().NotBeNull();
        result!.TariffName.Should().NotBeNullOrEmpty();
        result.TariffName.Should().Be(TestData.ExistingTariffs.Tariff1Name);
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnVisitWithAllProperties()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.ExistingTariffs.Tariff3Name, TestData.ExistingTariffs.Tariff3PricePerMinute);
        var visit = new Visit
        {
            UserId = TestData.NewVisits.NewVisit1UserId,
            TariffId = tariff.TariffId,
            EntryTime = DateTimeOffset.UtcNow,
            Status = VisitStatus.Active
        };
        Context.Visits.Add(visit);
        await Context.SaveChangesAsync();

        // Act
        var result = await VisitRepository.GetByIdAsync(visit.VisitId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(TestData.NewVisits.NewVisit1UserId);
        result.TariffId.Should().Be(tariff.TariffId);
        result.Status.Should().Be(VisitStatus.Active);
        result.EntryTime.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }
}
