namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

using Venue.TimeCafe.Test.Integration.Helpers;

public class GetActiveVisitsAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetActiveVisitsAsync_Should_ReturnAllActiveVisits()
    {
        // Arrange
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        await SeedVisitAsync(TestData.ExistingVisits.Visit2UserId);
        await SeedVisitAsync(TestData.ExistingVisits.Visit3UserId);

        // Act
        var result = await VisitRepository.GetActiveVisitsAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(v => v.Status == VisitStatus.Active);
    }

    [Fact]
    public async Task Repository_GetActiveVisitsAsync_Should_ReturnEmptyList_WhenNoActiveVisits()
    {
        // Arrange
        var visit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        visit.Status = VisitStatus.Completed;
        await Context.SaveChangesAsync();

        // Act
        var result = await VisitRepository.GetActiveVisitsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetActiveVisitsAsync_Should_ExcludeCompletedVisits()
    {
        // Arrange
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        var completedVisit = await SeedVisitAsync(TestData.ExistingVisits.Visit2UserId);
        completedVisit.Status = VisitStatus.Completed;
        await Context.SaveChangesAsync();

        // Act
        var result = await VisitRepository.GetActiveVisitsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().OnlyContain(v => v.Status == VisitStatus.Active);
    }

    [Fact]
    public async Task Repository_GetActiveVisitsAsync_Should_ReturnOrderedByEntryTimeDesc()
    {
        // Arrange
        var visit1 = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        await Task.Delay(100);
        var visit2 = await SeedVisitAsync(TestData.ExistingVisits.Visit2UserId);
        await Task.Delay(100);
        var visit3 = await SeedVisitAsync(TestData.ExistingVisits.Visit3UserId);

        // Act
        var result = (await VisitRepository.GetActiveVisitsAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].UserId.Should().Be(TestData.ExistingVisits.Visit3UserId);
        result[1].UserId.Should().Be(TestData.ExistingVisits.Visit2UserId);
        result[2].UserId.Should().Be(TestData.ExistingVisits.Visit1UserId);
    }

    [Fact]
    public async Task Repository_GetActiveVisitsAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        await VisitRepository.GetActiveVisitsAsync();
        await VisitRepository.GetActiveVisitsAsync();

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetActiveVisitsAsync_Should_IncludeTariffRelations()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.ExistingTariffs.Tariff3Name, TestData.ExistingTariffs.Tariff3PricePerMinute);
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId, tariff.TariffId);

        // Act
        var result = (await VisitRepository.GetActiveVisitsAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].TariffName.Should().NotBeNullOrEmpty();
        result[0].TariffName.Should().Be(TestData.ExistingTariffs.Tariff3Name);
    }
}
