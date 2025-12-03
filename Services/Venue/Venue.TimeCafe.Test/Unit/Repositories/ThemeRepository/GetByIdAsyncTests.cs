namespace Venue.TimeCafe.Test.Unit.Repositories.ThemeRepository;

public class GetByIdAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnTheme_WhenExists()
    {
        // Arrange
        var theme = await SeedThemeAsync("Test Theme");

        // Act
        var result = await ThemeRepository.GetByIdAsync(theme.ThemeId);

        // Assert
        result.Should().NotBeNull();
        result!.ThemeId.Should().Be(theme.ThemeId);
        result.Name.Should().Be("Test Theme");
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await ThemeRepository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        var theme = await SeedThemeAsync("Test Theme");
        await ThemeRepository.GetByIdAsync(theme.ThemeId);
        await ThemeRepository.GetByIdAsync(theme.ThemeId);

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnThemeWithProperties()
    {
        // Arrange
        var theme = new Theme
        {
            Name = "Full Theme",
            Emoji = "🎨",
            Colors = "#FF5733,#33FF57"
        };
        Context.Themes.Add(theme);
        await Context.SaveChangesAsync();

        // Act
        var result = await ThemeRepository.GetByIdAsync(theme.ThemeId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Full Theme");
        result.Emoji.Should().Be("🎨");
        result.Colors.Should().Be("#FF5733,#33FF57");
    }
}
