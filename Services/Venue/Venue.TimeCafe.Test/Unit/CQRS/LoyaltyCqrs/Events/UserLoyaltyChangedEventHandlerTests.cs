namespace Venue.TimeCafe.Test.Unit.CQRS.LoyaltyCqrs.Events;

public class UserLoyaltyChangedEventHandlerTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_Should_InvalidateUserLoyaltyCache()
    {
        // Arrange
        var loyalty = await SeedUserLoyaltyAsync(discount: 10m);
        var handler = new UserLoyaltyChangedEventHandler(HybridCache);

        // Fill cache
        await UserLoyaltyRepository.GetByIdAsync(loyalty.UserId);

        // Modify DB directly
        var dbLoyalty = await Context.UserLoyalties.FindAsync(loyalty.UserId);
        dbLoyalty!.PersonalDiscountPercent = 99m;
        await Context.SaveChangesAsync();

        // Act
        await handler.Handle(new UserLoyaltyChangedEvent(loyalty.UserId));

        // Assert
        var result = await UserLoyaltyRepository.GetByIdAsync(loyalty.UserId);
        result.Should().NotBeNull();
        result!.PersonalDiscountPercent.Should().Be(99m); // Should be NEW value because cache was invalidated
    }
}
