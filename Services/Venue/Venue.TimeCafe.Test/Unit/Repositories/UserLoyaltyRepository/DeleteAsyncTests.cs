namespace Venue.TimeCafe.Test.Unit.Repositories.UserLoyaltyRepository;

public class DeleteAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_DeleteAsync_Should_ReturnFalse_WhenNotFound()
    {
        // Act
        var result = await UserLoyaltyRepository.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_ReturnTrue_WhenDeleted()
    {
        // Arrange
        var loyalty = await SeedUserLoyaltyAsync();

        // Act
        var result = await UserLoyaltyRepository.DeleteAsync(loyalty.UserId);
        await Context.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
        var fromDb = await Context.UserLoyalties.FindAsync(loyalty.UserId);
        fromDb.Should().BeNull();
    }
}
