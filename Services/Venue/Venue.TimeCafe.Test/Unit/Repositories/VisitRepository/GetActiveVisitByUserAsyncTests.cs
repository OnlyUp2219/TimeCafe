namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

public class GetActiveVisitByUserAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetActiveVisitByUserAsync_Should_ReturnActiveVisit_WhenExists()
    {
        // Arrange
        await SeedVisitAsync("user123");

        // Act
        var result = await VisitRepository.GetActiveVisitByUserAsync("user123");

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be("user123");
        result.Status.Should().Be(VisitStatus.Active);
    }

    [Fact]
    public async Task Repository_GetActiveVisitByUserAsync_Should_ReturnNull_WhenNoActiveVisit()
    {
        // Arrange
        var visit = await SeedVisitAsync("user123");
        visit.Status = VisitStatus.Completed;
        await Context.SaveChangesAsync();

        // Act
        var result = await VisitRepository.GetActiveVisitByUserAsync("user123");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetActiveVisitByUserAsync_Should_ReturnNull_WhenUserNotExists()
    {
        // Act
        var result = await VisitRepository.GetActiveVisitByUserAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetActiveVisitByUserAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        await SeedVisitAsync("user123");
        await VisitRepository.GetActiveVisitByUserAsync("user123");
        await VisitRepository.GetActiveVisitByUserAsync("user123");

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetActiveVisitByUserAsync_Should_IncludeTariffRelation()
    {
        // Arrange
        var tariff = await SeedTariffAsync("Test Tariff", 150m);
        await SeedVisitAsync("user123", tariff.TariffId);

        // Act
        var result = await VisitRepository.GetActiveVisitByUserAsync("user123");

        // Assert
        result.Should().NotBeNull();
        result!.Tariff.Should().NotBeNull();
        result.Tariff.Name.Should().Be("Test Tariff");
    }

    [Fact]
    public async Task Repository_GetActiveVisitByUserAsync_Should_ReturnOnlyActiveVisit()
    {
        // Arrange
        var completedVisit = await SeedVisitAsync("user123");
        completedVisit.Status = VisitStatus.Completed;
        await Context.SaveChangesAsync();

        var activeVisit = await SeedVisitAsync("user123");

        // Act
        var result = await VisitRepository.GetActiveVisitByUserAsync("user123");

        // Assert
        result.Should().NotBeNull();
        result!.VisitId.Should().Be(activeVisit.VisitId);
        result.Status.Should().Be(VisitStatus.Active);
    }
}
