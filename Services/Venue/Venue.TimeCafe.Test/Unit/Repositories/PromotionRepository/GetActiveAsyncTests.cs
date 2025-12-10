namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class GetActiveAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetActiveAsync_Should_ReturnOnlyActivePromotions()
    {
        // Arrange
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, TestData.ExistingPromotions.Promotion1DiscountPercent, true);
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, TestData.ExistingPromotions.Promotion2DiscountPercent, true);
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion3Name, TestData.ExistingPromotions.Promotion3DiscountPercent, false);

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
        await SeedPromotionAsync(TestData.NewPromotions.NewPromotion1Name, TestData.NewPromotions.NewPromotion1DiscountPercent, false);
        await SeedPromotionAsync(TestData.NewPromotions.NewPromotion2Name, TestData.NewPromotions.NewPromotion2DiscountPercent, false);

        // Act
        var result = await PromotionRepository.GetActiveAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetActiveAsync_Should_ReturnOrderedByCreatedAtDesc()
    {
        // Arrange
        var promo1 = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, TestData.ExistingPromotions.Promotion1DiscountPercent, true);
        await Task.Delay(100);
        var promo2 = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, TestData.ExistingPromotions.Promotion2DiscountPercent, true);

        // Act
        var result = (await PromotionRepository.GetActiveAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be(TestData.ExistingPromotions.Promotion2Name);
        result[1].Name.Should().Be(TestData.ExistingPromotions.Promotion1Name);
    }

    [Fact]
    public async Task Repository_GetActiveAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        await SeedPromotionAsync(TestData.DefaultValues.DefaultPromotionName, TestData.DefaultValues.DefaultDiscountPercent, true);
        await PromotionRepository.GetActiveAsync();
        await PromotionRepository.GetActiveAsync();

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }
}
