namespace Venue.TimeCafe.Test.Unit.Repositories.ThemeRepository;

public class CreateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_CreateAsync_Should_ThrowException_WhenThemeIsNull()
    {
        // Arrange
        Theme? nullTheme = null;

        // Act
        var act = async () => await ThemeRepository.CreateAsync(nullTheme!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_CreateTheme_WhenValidData()
    {
        // Arrange
        var theme = new Theme
        {
            Name = "New Theme",
            Emoji = "🎯",
            Colors = "#123456"
        };

        // Act
        var result = await ThemeRepository.CreateAsync(theme);

        // Assert
        result.Should().NotBeNull();
        result.ThemeId.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Theme");
        result.Emoji.Should().Be("🎯");
        result.Colors.Should().Be("#123456");
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_PersistToDatabase()
    {
        // Arrange
        var theme = new Theme
        {
            Name = "Persisted Theme",
            Emoji = "💾"
        };

        // Act
        var result = await ThemeRepository.CreateAsync(theme);

        // Assert
        var fromDb = await Context.Themes.FindAsync(result.ThemeId);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be("Persisted Theme");
        fromDb.Emoji.Should().Be("💾");
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_InvalidateCache()
    {
        // Arrange
        var theme = new Theme { Name = "Cache Test" };

        // Act
        await ThemeRepository.CreateAsync(theme);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_CreateThemeWithMinimalData()
    {
        // Arrange
        var theme = new Theme { Name = "Minimal" };

        // Act
        var result = await ThemeRepository.CreateAsync(theme);

        // Assert
        result.Should().NotBeNull();
        result.ThemeId.Should().BeGreaterThan(0);
        result.Name.Should().Be("Minimal");
        result.Emoji.Should().BeNull();
        result.Colors.Should().BeNull();
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_CreateThemeWithAllProperties()
    {
        // Arrange
        var theme = new Theme
        {
            Name = "Full Theme",
            Emoji = "🔥",
            Colors = "#FF0000,#00FF00,#0000FF"
        };

        // Act
        var result = await ThemeRepository.CreateAsync(theme);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Full Theme");
        result.Emoji.Should().Be("🔥");
        result.Colors.Should().Be("#FF0000,#00FF00,#0000FF");
    }
}
