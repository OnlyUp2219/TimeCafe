namespace Venue.TimeCafe.Test.Unit.Repositories.UserLoyaltyRepository;

public class UpdateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_UpdateAsync_Should_ReturnNull_WhenNotFound()
    {
        // Arrange
        var loyalty = new UserLoyalty { UserId = Guid.NewGuid(), PersonalDiscountPercent = 10m };

        // Act
        var result = await UserLoyaltyRepository.UpdateAsync(loyalty);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateUserLoyalty_WhenExists()
    {
        // Arrange
        var loyalty = await SeedUserLoyaltyAsync(discount: 10m);
        loyalty.PersonalDiscountPercent = 20m;

        // Act
        var result = await UserLoyaltyRepository.UpdateAsync(loyalty);
        await Context.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        result!.PersonalDiscountPercent.Should().Be(20m);
        
        var fromDb = await Context.UserLoyalties.FindAsync(loyalty.UserId);
        fromDb.Should().NotBeNull();
        fromDb!.PersonalDiscountPercent.Should().Be(20m);
    }
}
