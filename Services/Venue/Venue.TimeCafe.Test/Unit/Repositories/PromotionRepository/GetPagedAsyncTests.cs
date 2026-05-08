namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class GetPagedAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetPagedAsync_Should_ReturnPagedPromotions()
    {
        // Arrange
        await SeedPromotionAsync("Promo A", 10m);
        await Task.Delay(10);
        await SeedPromotionAsync("Promo B", 20m);
        await Task.Delay(10);
        await SeedPromotionAsync("Promo C", 30m);

        // Act
        var result = (await PromotionRepository.GetPagedAsync(1, 2)).ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        // Ordered by CreatedAt descending, so Promo C should be first, then Promo B
        result[0].Name.Should().Be("Promo C");
        result[1].Name.Should().Be("Promo B");
    }

    [Fact]
    public async Task Repository_GetPagedAsync_Should_ReturnEmpty_WhenPageOutOfBounds()
    {
        // Arrange
        await SeedPromotionAsync("Promo A", 10m);

        // Act
        var result = await PromotionRepository.GetPagedAsync(2, 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetPagedAsync_Should_ReturnFromCache_WhenCached()
    {
        // Arrange
        var promo = await SeedPromotionAsync("Promo A", 10m);

        // First call fills cache
        await PromotionRepository.GetPagedAsync(1, 10);

        // Modify DB directly
        var dbPromo = await Context.Promotions.FindAsync(promo.PromotionId);
        dbPromo!.Name = "Updated Name";
        await Context.SaveChangesAsync();

        // Act
        var result = (await PromotionRepository.GetPagedAsync(1, 10)).ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Promo A"); // Should be old name from cache
    }
}
