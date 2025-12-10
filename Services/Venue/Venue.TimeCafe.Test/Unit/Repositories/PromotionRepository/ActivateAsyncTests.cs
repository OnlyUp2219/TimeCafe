namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class ActivateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_ActivateAsync_Should_ReturnTrue_WhenPromotionExists()
    {
        // Arrange
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, TestData.ExistingPromotions.Promotion1DiscountPercent, false);

        // Act
        var result = await PromotionRepository.ActivateAsync(promotion.PromotionId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_ActivateAsync_Should_SetIsActiveToTrue()
    {
        // Arrange
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, TestData.ExistingPromotions.Promotion2DiscountPercent, false);

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
        var nonExistentId = TestData.NonExistingIds.NonExistingPromotionId;

        // Act
        var result = await PromotionRepository.ActivateAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_ActivateAsync_Should_InvalidateCache()
    {
        // Arrange
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion3Name, TestData.ExistingPromotions.Promotion3DiscountPercent, false);

        // Act
        await PromotionRepository.ActivateAsync(promotion.PromotionId);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
