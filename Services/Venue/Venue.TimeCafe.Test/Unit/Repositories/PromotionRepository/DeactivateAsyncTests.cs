namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class DeactivateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_DeactivateAsync_Should_ReturnTrue_WhenPromotionExists()
    {
        // Arrange
        var promotion = await SeedPromotionAsync("Test", 10m, true);

        // Act
        var result = await PromotionRepository.DeactivateAsync(promotion.PromotionId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_DeactivateAsync_Should_SetIsActiveToFalse()
    {
        // Arrange
        var promotion = await SeedPromotionAsync("Active", 10m, true);

        // Act
        await PromotionRepository.DeactivateAsync(promotion.PromotionId);

        // Assert
        var fromDb = await Context.Promotions.FindAsync(promotion.PromotionId);
        fromDb.Should().NotBeNull();
        fromDb!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_DeactivateAsync_Should_ReturnFalse_WhenPromotionNotExists()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await PromotionRepository.DeactivateAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_DeactivateAsync_Should_InvalidateCache()
    {
        // Arrange
        var promotion = await SeedPromotionAsync("Test", 10m, true);

        // Act
        await PromotionRepository.DeactivateAsync(promotion.PromotionId);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
