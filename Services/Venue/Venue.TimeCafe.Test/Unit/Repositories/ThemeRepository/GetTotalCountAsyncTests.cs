namespace Venue.TimeCafe.Test.Unit.Repositories.ThemeRepository;

public class GetTotalCountAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_ReturnCorrectCount()
    {
        // Arrange
        await SeedThemeAsync("Theme 1");
        await SeedThemeAsync("Theme 2");

        // Act
        var result = await ThemeRepository.GetTotalCountAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_ReturnZero_WhenNoThemes()
    {
        // Act
        var result = await ThemeRepository.GetTotalCountAsync();

        // Assert
        result.Should().Be(0);
    }
}
