namespace Venue.TimeCafe.Test.Unit.CQRS.PromotionsCqrs.Events;

public class PromotionChangedEventHandlerTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_Should_InvalidatePromotionsCache()
    {
        // Arrange
        var promo = await SeedPromotionAsync("Promo 1", 10m);
        var handler = new PromotionChangedEventHandler(HybridCache);

        // Fill cache
        await PromotionRepository.GetPagedAsync(1, 10);

        // Modify DB directly
        var dbPromo = await Context.Promotions.FindAsync(promo.PromotionId);
        dbPromo!.Name = "Updated Name";
        await Context.SaveChangesAsync();

        // Act
        await handler.Handle(new PromotionChangedEvent(promo.PromotionId));

        // Assert
        var result = (await PromotionRepository.GetPagedAsync(1, 10)).ToList();
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Updated Name"); // Should be NEW name because cache was invalidated
    }
}
