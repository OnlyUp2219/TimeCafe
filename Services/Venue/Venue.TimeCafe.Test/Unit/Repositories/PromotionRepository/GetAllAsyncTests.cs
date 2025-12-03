namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class GetAllAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnAllPromotions()
    {
        // Arrange
        await SeedPromotionAsync("Promo 1", 10m);
        await SeedPromotionAsync("Promo 2", 20m);
        await SeedPromotionAsync("Promo 3", 30m);

        // Act
        var result = await PromotionRepository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnEmptyList_WhenNoPromotions()
    {
        // Act
        var result = await PromotionRepository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnOrderedByCreatedAtDesc()
    {
        // Arrange
        var promo1 = await SeedPromotionAsync("First", 10m);
        await Task.Delay(100);
        var promo2 = await SeedPromotionAsync("Second", 20m);
        await Task.Delay(100);
        var promo3 = await SeedPromotionAsync("Third", 30m);

        // Act
        var result = (await PromotionRepository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Third");
        result[1].Name.Should().Be("Second");
        result[2].Name.Should().Be("First");
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        await SeedPromotionAsync("Promo1", 10m);
        await SeedPromotionAsync("Promo2", 20m);
        await PromotionRepository.GetAllAsync();
        await PromotionRepository.GetAllAsync();

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnBothActiveAndInactive()
    {
        // Arrange
        await SeedPromotionAsync("Active", 10m, true);
        await SeedPromotionAsync("Inactive", 20m, false);

        // Act
        var result = await PromotionRepository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.IsActive);
        result.Should().Contain(p => !p.IsActive);
    }
}
