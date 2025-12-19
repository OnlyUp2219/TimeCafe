namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

public class GetVisitHistoryByUserAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetVisitHistoryByUserAsync_Should_ReturnUserVisits()
    {
        // Arrange
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        await SeedVisitAsync(TestData.ExistingVisits.Visit2UserId);

        // Act
        var result = await VisitRepository.GetVisitHistoryByUserAsync(TestData.ExistingVisits.Visit1UserId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(v => v.UserId == TestData.ExistingVisits.Visit1UserId);
    }

    [Fact]
    public async Task Repository_GetVisitHistoryByUserAsync_Should_ReturnEmptyList_WhenUserHasNoVisits()
    {
        // Act
        var result = await VisitRepository.GetVisitHistoryByUserAsync(TestData.NonExistingIds.NonExistingUserId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetVisitHistoryByUserAsync_Should_ReturnOrderedByEntryTimeDesc()
    {
        // Arrange
        var visit1 = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        await Task.Delay(100);
        var visit2 = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        await Task.Delay(100);
        var visit3 = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);

        // Act
        var result = (await VisitRepository.GetVisitHistoryByUserAsync(TestData.ExistingVisits.Visit1UserId)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].VisitId.Should().Be(visit3.VisitId);
        result[1].VisitId.Should().Be(visit2.VisitId);
        result[2].VisitId.Should().Be(visit1.VisitId);
    }

    [Fact]
    public async Task Repository_GetVisitHistoryByUserAsync_Should_ReturnPaginatedResults()
    {
        // Arrange
        for (int i = 0; i < 25; i++)
        {
            await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        }

        // Act
        var page1 = await VisitRepository.GetVisitHistoryByUserAsync(TestData.ExistingVisits.Visit1UserId, pageNumber: 1, pageSize: TestData.DefaultValues.DefaultPageSize);
        var page2 = await VisitRepository.GetVisitHistoryByUserAsync(TestData.ExistingVisits.Visit1UserId, pageNumber: 2, pageSize: TestData.DefaultValues.DefaultPageSize);

        // Assert
        page1.Should().HaveCount(TestData.DefaultValues.DefaultPageSize);
        page2.Should().HaveCount(TestData.DefaultValues.DefaultPageSize);
    }

    [Fact]
    public async Task Repository_GetVisitHistoryByUserAsync_Should_IncludeTariffRelations()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.ExistingTariffs.Tariff2Name, TestData.ExistingTariffs.Tariff2PricePerMinute);
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId, tariff.TariffId);

        // Act
        var result = (await VisitRepository.GetVisitHistoryByUserAsync(TestData.ExistingVisits.Visit1UserId)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].TariffName.Should().NotBeNullOrEmpty();
        result[0].TariffName.Should().Be(TestData.ExistingTariffs.Tariff2Name);
    }

    [Fact]
    public async Task Repository_GetVisitHistoryByUserAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        await VisitRepository.GetVisitHistoryByUserAsync(TestData.ExistingVisits.Visit1UserId);
        await VisitRepository.GetVisitHistoryByUserAsync(TestData.ExistingVisits.Visit1UserId);

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetVisitHistoryByUserAsync_Should_IncludeBothActiveAndCompleted()
    {
        // Arrange
        var activeVisit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        var completedVisit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        completedVisit.Status = VisitStatus.Completed;
        await Context.SaveChangesAsync();

        // Act
        var result = await VisitRepository.GetVisitHistoryByUserAsync(TestData.ExistingVisits.Visit1UserId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(v => v.Status == VisitStatus.Active);
        result.Should().Contain(v => v.Status == VisitStatus.Completed);
    }
}
