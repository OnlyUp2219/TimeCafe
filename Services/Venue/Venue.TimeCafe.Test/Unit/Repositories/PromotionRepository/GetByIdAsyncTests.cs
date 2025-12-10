namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class GetByIdAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnPromotion_WhenExists()
    {
        // Arrange
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, TestData.ExistingPromotions.Promotion1DiscountPercent);

        // Act
        var result = await PromotionRepository.GetByIdAsync(promotion.PromotionId);

        // Assert
        result.Should().NotBeNull();
        result!.PromotionId.Should().Be(promotion.PromotionId);
        result.Name.Should().Be(TestData.ExistingPromotions.Promotion1Name);
        result.DiscountPercent.Should().Be(TestData.ExistingPromotions.Promotion1DiscountPercent);
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = TestData.NonExistingIds.NonExistingPromotionId;

        // Act
        var result = await PromotionRepository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        var promotion = await SeedPromotionAsync(TestData.DefaultValues.DefaultPromotionName, TestData.DefaultValues.DefaultDiscountPercent);
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
            Name = TestData.ExistingPromotions.Promotion2Name,
            Description = TestData.ExistingPromotions.Promotion2Description,
            DiscountPercent = TestData.ExistingPromotions.Promotion2DiscountPercent,
            ValidFrom = TestData.DateTimeData.GetValidFromDate(),
            ValidTo = TestData.DateTimeData.GetValidToDate(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        Context.Promotions.Add(promotion);
        await Context.SaveChangesAsync();

        // Act
        var result = await PromotionRepository.GetByIdAsync(promotion.PromotionId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(TestData.ExistingPromotions.Promotion2Name);
        result.Description.Should().Be(TestData.ExistingPromotions.Promotion2Description);
        result.DiscountPercent.Should().Be(TestData.ExistingPromotions.Promotion2DiscountPercent);
        result.IsActive.Should().BeTrue();
    }
}
