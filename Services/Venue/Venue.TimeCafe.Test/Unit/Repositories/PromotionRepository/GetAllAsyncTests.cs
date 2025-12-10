namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class GetAllAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnAllPromotions()
    {
        // Arrange
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, TestData.ExistingPromotions.Promotion1DiscountPercent);
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, TestData.ExistingPromotions.Promotion2DiscountPercent);
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion3Name, TestData.ExistingPromotions.Promotion3DiscountPercent);

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
        var promo1 = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, TestData.ExistingPromotions.Promotion1DiscountPercent);
        await Task.Delay(100);
        var promo2 = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, TestData.ExistingPromotions.Promotion2DiscountPercent);
        await Task.Delay(100);
        var promo3 = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion3Name, TestData.ExistingPromotions.Promotion3DiscountPercent);

        // Act
        var result = (await PromotionRepository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be(TestData.ExistingPromotions.Promotion3Name);
        result[1].Name.Should().Be(TestData.ExistingPromotions.Promotion2Name);
        result[2].Name.Should().Be(TestData.ExistingPromotions.Promotion1Name);
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        await SeedPromotionAsync(TestData.NewPromotions.NewPromotion1Name, TestData.NewPromotions.NewPromotion1DiscountPercent);
        await SeedPromotionAsync(TestData.NewPromotions.NewPromotion2Name, TestData.NewPromotions.NewPromotion2DiscountPercent);
        await PromotionRepository.GetAllAsync();
        await PromotionRepository.GetAllAsync();

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnBothActiveAndInactive()
    {
        // Arrange
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, TestData.ExistingPromotions.Promotion1DiscountPercent, true);
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, TestData.ExistingPromotions.Promotion2DiscountPercent, false);

        // Act
        var result = await PromotionRepository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.IsActive);
        result.Should().Contain(p => !p.IsActive);
    }
}
