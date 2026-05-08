namespace Venue.TimeCafe.Test.Unit.Repositories.UserLoyaltyRepository;

public class GetByIdAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnNull_WhenNotFound()
    {
        // Act
        var result = await UserLoyaltyRepository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnUserLoyalty_WhenExists()
    {
        // Arrange
        var loyalty = await SeedUserLoyaltyAsync();

        // Act
        var result = await UserLoyaltyRepository.GetByIdAsync(loyalty.UserId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(loyalty.UserId);
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnFromCache_WhenCached()
    {
        // Arrange
        var loyalty = await SeedUserLoyaltyAsync(discount: 10m);

        // First call fills cache
        await UserLoyaltyRepository.GetByIdAsync(loyalty.UserId);

        // Modify DB directly to see if cache is used
        var dbLoyalty = await Context.UserLoyalties.FindAsync(loyalty.UserId);
        dbLoyalty!.PersonalDiscountPercent = 99m;
        await Context.SaveChangesAsync();

        // Act
        var result = await UserLoyaltyRepository.GetByIdAsync(loyalty.UserId);

        // Assert
        result.Should().NotBeNull();
        result!.PersonalDiscountPercent.Should().Be(10m); // Should be old value from cache
        result.PersonalDiscountPercent.Should().NotBe(99m);
    }
}
