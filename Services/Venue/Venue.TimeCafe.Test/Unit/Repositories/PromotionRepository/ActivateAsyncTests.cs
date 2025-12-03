namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class ActivateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_ActivateAsync_Should_ReturnTrue_WhenPromotionExists()
    {
        // Arrange
        var promotion = await SeedPromotionAsync("Test", 10m, false);

        // Act
        var result = await PromotionRepository.ActivateAsync(promotion.PromotionId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_ActivateAsync_Should_SetIsActiveToTrue()
    {
        // Arrange
        var promotion = await SeedPromotionAsync("Inactive", 10m, false);

        // Act
        await PromotionRepository.ActivateAsync(promotion.PromotionId);

        // Assert
        var fromDb = await Context.Promotions.FindAsync(promotion.PromotionId);
        fromDb.Should().NotBeNull();
        fromDb!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_ActivateAsync_Should_ReturnFalse_WhenPromotionNotExists()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await PromotionRepository.ActivateAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_ActivateAsync_Should_InvalidateCache()
    {
        // Arrange
        var promotion = await SeedPromotionAsync("Test", 10m, false);

        // Act
        await PromotionRepository.ActivateAsync(promotion.PromotionId);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
