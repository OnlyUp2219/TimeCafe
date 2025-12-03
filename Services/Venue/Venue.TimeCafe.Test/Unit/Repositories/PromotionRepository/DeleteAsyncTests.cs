namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class DeleteAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_DeleteAsync_Should_ReturnTrue_WhenPromotionExists()
    {
        // Arrange
        var promotion = await SeedPromotionAsync("To Delete", 10m);

        // Act
        var result = await PromotionRepository.DeleteAsync(promotion.PromotionId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_ReturnFalse_WhenPromotionNotExists()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await PromotionRepository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_RemoveFromDatabase()
    {
        // Arrange
        var promotion = await SeedPromotionAsync("To Remove", 10m);
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
        var promotion = await SeedPromotionAsync("Cache Test", 10m);

        // Act
        await PromotionRepository.DeleteAsync(promotion.PromotionId);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_HandleAlreadyDeleted()
    {
        // Arrange
        var promotion = await SeedPromotionAsync("To Delete Twice", 10m);
        var promotionId = promotion.PromotionId;

        // Act
        var firstDelete = await PromotionRepository.DeleteAsync(promotionId);
        var secondDelete = await PromotionRepository.DeleteAsync(promotionId);

        // Assert
        firstDelete.Should().BeTrue();
        secondDelete.Should().BeFalse();
    }
}
