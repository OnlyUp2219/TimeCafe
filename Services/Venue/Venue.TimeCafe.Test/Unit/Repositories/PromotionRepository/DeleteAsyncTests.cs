namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class DeleteAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_DeleteAsync_Should_ReturnTrue_WhenPromotionExists()
    {
        // Arrange
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, TestData.ExistingPromotions.Promotion1DiscountPercent);

        // Act
        var result = await PromotionRepository.DeleteAsync(promotion.PromotionId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_ReturnFalse_WhenPromotionNotExists()
    {
        // Arrange
        var nonExistentId = TestData.NonExistingIds.NonExistingPromotionId;

        // Act
        var result = await PromotionRepository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_RemoveFromDatabase()
    {
        // Arrange
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, TestData.ExistingPromotions.Promotion2DiscountPercent);
        var promotionId = promotion.PromotionId;

        // Act
        await PromotionRepository.DeleteAsync(promotionId);

        // Assert
        var fromDb = await Context.Promotions.FindAsync(promotionId);
        fromDb.Should().BeNull();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_InvalidateCache()
    {
        // Arrange
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion3Name, TestData.ExistingPromotions.Promotion3DiscountPercent);

        // Act
        await PromotionRepository.DeleteAsync(promotion.PromotionId);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_HandleAlreadyDeleted()
    {
        // Arrange
        var promotion = await SeedPromotionAsync(TestData.NewPromotions.NewPromotion1Name, TestData.NewPromotions.NewPromotion1DiscountPercent);
        var promotionId = promotion.PromotionId;

        // Act
        var firstDelete = await PromotionRepository.DeleteAsync(promotionId);
        var secondDelete = await PromotionRepository.DeleteAsync(promotionId);

        // Assert
        firstDelete.Should().BeTrue();
        secondDelete.Should().BeFalse();
    }
}
