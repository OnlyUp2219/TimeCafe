namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

public class GetVisitHistoryByUserAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetVisitHistoryByUserAsync_Should_ReturnUserVisits()
    {
        // Arrange
        await SeedVisitAsync("user123");
        await SeedVisitAsync("user123");
        await SeedVisitAsync("other_user");

        // Act
        var result = await VisitRepository.GetVisitHistoryByUserAsync("user123");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(v => v.UserId == "user123");
    }

    [Fact]
    public async Task Repository_GetVisitHistoryByUserAsync_Should_ReturnEmptyList_WhenUserHasNoVisits()
    {
        // Act
        var result = await VisitRepository.GetVisitHistoryByUserAsync("nonexistent");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetVisitHistoryByUserAsync_Should_ReturnOrderedByEntryTimeDesc()
    {
        // Arrange
        var visit1 = await SeedVisitAsync("user123");
        await Task.Delay(100);
        var visit2 = await SeedVisitAsync("user123");
        await Task.Delay(100);
        var visit3 = await SeedVisitAsync("user123");

        // Act
        var result = (await VisitRepository.GetVisitHistoryByUserAsync("user123")).ToList();

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
            await SeedVisitAsync("user123");
        }

        // Act
        var page1 = await VisitRepository.GetVisitHistoryByUserAsync("user123", pageNumber: 1, pageSize: 10);
        var page2 = await VisitRepository.GetVisitHistoryByUserAsync("user123", pageNumber: 2, pageSize: 10);

        // Assert
        page1.Should().HaveCount(10);
        page2.Should().HaveCount(10);
    }

    [Fact]
    public async Task Repository_GetVisitHistoryByUserAsync_Should_IncludeTariffRelations()
    {
        // Arrange
        var tariff = await SeedTariffAsync("Standard", 100m);
        await SeedVisitAsync("user123", tariff.TariffId);

        // Act
        var result = (await VisitRepository.GetVisitHistoryByUserAsync("user123")).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Tariff.Should().NotBeNull();
        result[0].Tariff.Name.Should().Be("Standard");
    }

    [Fact]
    public async Task Repository_GetVisitHistoryByUserAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        await SeedVisitAsync("user123");
        await VisitRepository.GetVisitHistoryByUserAsync("user123");
        await VisitRepository.GetVisitHistoryByUserAsync("user123");

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetVisitHistoryByUserAsync_Should_IncludeBothActiveAndCompleted()
    {
        // Arrange
        var activeVisit = await SeedVisitAsync("user123");
        var completedVisit = await SeedVisitAsync("user123");
        completedVisit.Status = VisitStatus.Completed;
        await Context.SaveChangesAsync();

        // Act
        var result = await VisitRepository.GetVisitHistoryByUserAsync("user123");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(v => v.Status == VisitStatus.Active);
        result.Should().Contain(v => v.Status == VisitStatus.Completed);
    }
}
