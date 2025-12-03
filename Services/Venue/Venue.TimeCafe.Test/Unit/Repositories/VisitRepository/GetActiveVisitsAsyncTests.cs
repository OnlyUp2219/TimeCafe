namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

public class GetActiveVisitsAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetActiveVisitsAsync_Should_ReturnAllActiveVisits()
    {
        // Arrange
        await SeedVisitAsync("user1");
        await SeedVisitAsync("user2");
        await SeedVisitAsync("user3");

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
        var visit = await SeedVisitAsync("user1");
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
        await SeedVisitAsync("user1");
        var completedVisit = await SeedVisitAsync("user2");
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
        var visit1 = await SeedVisitAsync("user1");
        await Task.Delay(100);
        var visit2 = await SeedVisitAsync("user2");
        await Task.Delay(100);
        var visit3 = await SeedVisitAsync("user3");

        // Act
        var result = (await VisitRepository.GetActiveVisitsAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].UserId.Should().Be("user3");
        result[1].UserId.Should().Be("user2");
        result[2].UserId.Should().Be("user1");
    }

    [Fact]
    public async Task Repository_GetActiveVisitsAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        await SeedVisitAsync("user1");
        await VisitRepository.GetActiveVisitsAsync();
        await VisitRepository.GetActiveVisitsAsync();

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetActiveVisitsAsync_Should_IncludeTariffRelations()
    {
        // Arrange
        var tariff = await SeedTariffAsync("Premium", 200m);
        await SeedVisitAsync("user1", tariff.TariffId);

        // Act
        var result = (await VisitRepository.GetActiveVisitsAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Tariff.Should().NotBeNull();
        result[0].Tariff.Name.Should().Be("Premium");
    }
}
