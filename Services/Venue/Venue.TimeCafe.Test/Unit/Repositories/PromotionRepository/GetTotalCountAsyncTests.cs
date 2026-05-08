namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class GetTotalCountAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_ReturnCorrectCount()
    {
        // Arrange
        await SeedPromotionAsync("Promo 1", 10m);
        await SeedPromotionAsync("Promo 2", 20m);

        // Act
        var result = await PromotionRepository.GetTotalCountAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_ReturnZero_WhenNoPromotions()
    {
        // Act
        var result = await PromotionRepository.GetTotalCountAsync();

        // Assert
        result.Should().Be(0);
    }
}
