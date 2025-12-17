namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class UpdateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_UpdateAsync_Should_ThrowException_WhenPromotionIsNull()
    {
        // Arrange
        Promotion? nullPromotion = null;

        // Act
        var act = async () => await PromotionRepository.UpdateAsync(nullPromotion!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdatePromotion_WhenExists()
    {
        // Arrange
        var existing = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, TestData.ExistingPromotions.Promotion1DiscountPercent);
        existing.Name = TestData.UpdateData.UpdatedPromotionName;
        existing.DiscountPercent = TestData.UpdateData.UpdatedDiscountPercent;
        existing.Description = TestData.UpdateData.UpdatedPromotionDescription;

        // Act
        var result = await PromotionRepository.UpdateAsync(existing);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(TestData.UpdateData.UpdatedPromotionName);
        result.DiscountPercent.Should().Be(TestData.UpdateData.UpdatedDiscountPercent);
        result.Description.Should().Be(TestData.UpdateData.UpdatedPromotionDescription);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistent = new Promotion
        {
            PromotionId = TestData.NonExistingIds.NonExistingPromotionId,
            Name = TestData.ExistingPromotions.Promotion1Name,
            Description = TestData.DefaultValues.DefaultPromotionDescription,
            DiscountPercent = TestData.DefaultValues.DefaultDiscountPercent,
            ValidFrom = TestData.DateTimeData.GetValidFromDate(),
            ValidTo = TestData.DateTimeData.GetValidToDate()
        };

        // Act
        var result = await PromotionRepository.UpdateAsync(nonExistent);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_InvalidateCache()
    {
        // Arrange
        var existing = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, TestData.ExistingPromotions.Promotion1DiscountPercent);
        existing.Name = TestData.UpdateData.UpdatedPromotionName;

        // Act
        await PromotionRepository.UpdateAsync(existing);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_PersistChanges()
    {
        // Arrange
        var existing = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, TestData.ExistingPromotions.Promotion2DiscountPercent);
        existing.Name = TestData.UpdateData.UpdatedPromotionName;
        existing.DiscountPercent = TestData.UpdateData.UpdatedDiscountPercent;

        // Act
        await PromotionRepository.UpdateAsync(existing);

        // Assert
        var fromDb = await Context.Promotions.FindAsync(existing.PromotionId);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be(TestData.UpdateData.UpdatedPromotionName);
        fromDb.DiscountPercent.Should().Be(TestData.UpdateData.UpdatedDiscountPercent);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateDateRange()
    {
        // Arrange
        var existing = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion3Name, TestData.ExistingPromotions.Promotion3DiscountPercent);
        var newValidFrom = TestData.DateTimeData.GetFutureDate();
        var newValidTo = TestData.DateTimeData.GetFutureDate().AddDays(30);
        existing.ValidFrom = newValidFrom;
        existing.ValidTo = newValidTo;

        // Act
        var result = await PromotionRepository.UpdateAsync(existing);

        // Assert
        result.ValidFrom.Should().BeCloseTo(newValidFrom, TimeSpan.FromSeconds(1));
        result.ValidTo.Should().BeCloseTo(newValidTo, TimeSpan.FromSeconds(1));
    }
}
