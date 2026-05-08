namespace Venue.TimeCafe.Test.Unit.Repositories.UserLoyaltyRepository;

public class CreateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_CreateAsync_Should_ThrowException_WhenUserLoyaltyIsNull()
    {
        // Arrange
        UserLoyalty? nullLoyalty = null;

        // Act
        var act = async () => await UserLoyaltyRepository.CreateAsync(nullLoyalty!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_CreateUserLoyalty_WhenValidData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var loyalty = new UserLoyalty
        {
            UserId = userId,
            PersonalDiscountPercent = 15m
        };

        // Act
        var result = await UserLoyaltyRepository.CreateAsync(loyalty);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.PersonalDiscountPercent.Should().Be(15m);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_PersistToDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var loyalty = new UserLoyalty
        {
            UserId = userId,
            PersonalDiscountPercent = 20m
        };

        // Act
        var result = await UserLoyaltyRepository.CreateAsync(loyalty);
        await Context.SaveChangesAsync();

        // Assert
        var fromDb = await Context.UserLoyalties.FindAsync(result.UserId);
        fromDb.Should().NotBeNull();
        fromDb!.UserId.Should().Be(userId);
        fromDb.PersonalDiscountPercent.Should().Be(20m);
    }
}
