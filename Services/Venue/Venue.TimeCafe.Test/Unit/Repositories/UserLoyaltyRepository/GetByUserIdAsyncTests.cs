namespace Venue.TimeCafe.Test.Unit.Repositories.UserLoyaltyRepository;

public class GetByUserIdAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetByUserIdAsync_Should_ReturnNull_WhenNotFound()
    {
        // Act
        var result = await UserLoyaltyRepository.GetByUserIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByUserIdAsync_Should_ReturnUserLoyalty_WhenExists()
    {
        // Arrange
        var loyalty = await SeedUserLoyaltyAsync();

        // Act
        var result = await UserLoyaltyRepository.GetByUserIdAsync(loyalty.UserId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(loyalty.UserId);
    }
}
