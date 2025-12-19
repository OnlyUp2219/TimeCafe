namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

public class GetActiveVisitByUserAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetActiveVisitByUserAsync_Should_ReturnActiveVisit_WhenExists()
    {
        // Arrange
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);

        // Act
        var result = await VisitRepository.GetActiveVisitByUserAsync(TestData.ExistingVisits.Visit1UserId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(TestData.ExistingVisits.Visit1UserId);
        result.Status.Should().Be(VisitStatus.Active);
    }

    [Fact]
    public async Task Repository_GetActiveVisitByUserAsync_Should_ReturnNull_WhenNoActiveVisit()
    {
        // Arrange
        var visit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        visit.Status = VisitStatus.Completed;
        await Context.SaveChangesAsync();

        // Act
        var result = await VisitRepository.GetActiveVisitByUserAsync(TestData.ExistingVisits.Visit1UserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetActiveVisitByUserAsync_Should_ReturnNull_WhenUserNotExists()
    {
        // Act
        var result = await VisitRepository.GetActiveVisitByUserAsync(TestData.NonExistingIds.NonExistingUserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetActiveVisitByUserAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        await VisitRepository.GetActiveVisitByUserAsync(TestData.ExistingVisits.Visit1UserId);
        await VisitRepository.GetActiveVisitByUserAsync(TestData.ExistingVisits.Visit1UserId);

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetActiveVisitByUserAsync_Should_IncludeTariffRelation()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId, tariff.TariffId);

        // Act
        var result = await VisitRepository.GetActiveVisitByUserAsync(TestData.ExistingVisits.Visit1UserId);

        // Assert
        result.Should().NotBeNull();
        result!.TariffName.Should().NotBeNullOrEmpty();
        result.TariffName.Should().Be(TestData.ExistingTariffs.Tariff1Name);
    }

    [Fact]
    public async Task Repository_GetActiveVisitByUserAsync_Should_ReturnOnlyActiveVisit()
    {
        // Arrange
        var completedVisit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        completedVisit.Status = VisitStatus.Completed;
        await Context.SaveChangesAsync();

        var activeVisit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);

        // Act
        var result = await VisitRepository.GetActiveVisitByUserAsync(TestData.ExistingVisits.Visit1UserId);

        // Assert
        result.Should().NotBeNull();
        result!.VisitId.Should().Be(activeVisit.VisitId);
        result.Status.Should().Be(VisitStatus.Active);
    }
}
