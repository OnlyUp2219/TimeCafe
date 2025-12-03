namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class GetByIdAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnPromotion_WhenExists()
    {
        // Arrange
        var promotion = await SeedPromotionAsync("Test Promo", 10m);

        // Act
        var result = await PromotionRepository.GetByIdAsync(promotion.PromotionId);

        // Assert
        result.Should().NotBeNull();
        result!.PromotionId.Should().Be(promotion.PromotionId);
        result.Name.Should().Be("Test Promo");
        result.DiscountPercent.Should().Be(10m);
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await PromotionRepository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        var promotion = await SeedPromotionAsync("Test", 15m);
        await PromotionRepository.GetByIdAsync(promotion.PromotionId);
        await PromotionRepository.GetByIdAsync(promotion.PromotionId);

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnPromotionWithAllProperties()
    {
        // Arrange
        var promotion = new Promotion
        {
            Name = "Full Promo",
            Description = "Test Description",
            DiscountPercent = 25m,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        Context.Promotions.Add(promotion);
        await Context.SaveChangesAsync();

        // Act
        var result = await PromotionRepository.GetByIdAsync(promotion.PromotionId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Full Promo");
        result.Description.Should().Be("Test Description");
        result.DiscountPercent.Should().Be(25m);
        result.IsActive.Should().BeTrue();
    }
}
