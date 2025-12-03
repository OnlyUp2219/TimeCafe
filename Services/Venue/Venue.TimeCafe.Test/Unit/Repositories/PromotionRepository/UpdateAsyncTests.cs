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
        var existing = await SeedPromotionAsync("Original", 10m);
        existing.Name = "Updated";
        existing.DiscountPercent = 25m;
        existing.Description = "Updated Description";

        // Act
        var result = await PromotionRepository.UpdateAsync(existing);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated");
        result.DiscountPercent.Should().Be(25m);
        result.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistent = new Promotion
        {
            PromotionId = 99999,
            Name = "Non-existent",
            Description = "Test",
            DiscountPercent = 10m,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(7)
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
        var existing = await SeedPromotionAsync("Original", 10m);
        existing.Name = "Updated";

        // Act
        await PromotionRepository.UpdateAsync(existing);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_PersistChanges()
    {
        // Arrange
        var existing = await SeedPromotionAsync("Original", 10m);
        existing.Name = "Persisted Update";
        existing.DiscountPercent = 30m;

        // Act
        await PromotionRepository.UpdateAsync(existing);

        // Assert
        var fromDb = await Context.Promotions.FindAsync(existing.PromotionId);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be("Persisted Update");
        fromDb.DiscountPercent.Should().Be(30m);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateDateRange()
    {
        // Arrange
        var existing = await SeedPromotionAsync("Original", 10m);
        var newValidFrom = DateTime.UtcNow.AddDays(5);
        var newValidTo = DateTime.UtcNow.AddDays(15);
        existing.ValidFrom = newValidFrom;
        existing.ValidTo = newValidTo;

        // Act
        var result = await PromotionRepository.UpdateAsync(existing);

        // Assert
        result.ValidFrom.Should().BeCloseTo(newValidFrom, TimeSpan.FromSeconds(1));
        result.ValidTo.Should().BeCloseTo(newValidTo, TimeSpan.FromSeconds(1));
    }
}
