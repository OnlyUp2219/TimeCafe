namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class GetActiveAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetActiveAsync_Should_ReturnOnlyActivePromotions()
    {
        // Arrange
        await SeedPromotionAsync("Active 1", 10m, true);
        await SeedPromotionAsync("Active 2", 20m, true);
        await SeedPromotionAsync("Inactive", 30m, false);

        // Act
        var result = await PromotionRepository.GetActiveAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.IsActive);
    }

    [Fact]
    public async Task Repository_GetActiveAsync_Should_ReturnEmptyList_WhenNoActivePromotions()
    {
        // Arrange
        await SeedPromotionAsync("Inactive 1", 10m, false);
        await SeedPromotionAsync("Inactive 2", 20m, false);

        // Act
        var result = await PromotionRepository.GetActiveAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetActiveAsync_Should_ReturnOrderedByCreatedAtDesc()
    {
        // Arrange
        var promo1 = await SeedPromotionAsync("First", 10m, true);
        await Task.Delay(100);
        var promo2 = await SeedPromotionAsync("Second", 20m, true);

        // Act
        var result = (await PromotionRepository.GetActiveAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Second");
        result[1].Name.Should().Be("First");
    }

    [Fact]
    public async Task Repository_GetActiveAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        await SeedPromotionAsync("Active", 10m, true);
        await PromotionRepository.GetActiveAsync();
        await PromotionRepository.GetActiveAsync();

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }
}
