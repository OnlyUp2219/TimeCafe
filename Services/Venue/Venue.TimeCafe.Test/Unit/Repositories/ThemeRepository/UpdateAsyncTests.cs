namespace Venue.TimeCafe.Test.Unit.Repositories.ThemeRepository;

public class UpdateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_UpdateAsync_Should_ThrowException_WhenThemeIsNull()
    {
        // Arrange
        Theme? nullTheme = null;

        // Act
        var act = async () => await ThemeRepository.UpdateAsync(nullTheme!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateTheme_WhenExists()
    {
        // Arrange
        var existing = await SeedThemeAsync("Original");
        existing.Name = "Updated";
        existing.Emoji = "✏️";
        existing.Colors = "#UPDATED";

        // Act
        var result = await ThemeRepository.UpdateAsync(existing);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated");
        result.Emoji.Should().Be("✏️");
        result.Colors.Should().Be("#UPDATED");
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistent = new Theme
        {
            ThemeId = 99999,
            Name = "Non-existent"
        };

        // Act
        var result = await ThemeRepository.UpdateAsync(nonExistent);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_InvalidateCache()
    {
        // Arrange
        var existing = await SeedThemeAsync("Original");
        existing.Name = "Updated";

        // Act
        await ThemeRepository.UpdateAsync(existing);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_PersistChanges()
    {
        // Arrange
        var existing = await SeedThemeAsync("Original");
        existing.Name = "Persisted Update";
        existing.Emoji = "📝";

        // Act
        await ThemeRepository.UpdateAsync(existing);

        // Assert
        var fromDb = await Context.Themes.FindAsync(existing.ThemeId);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be("Persisted Update");
        fromDb.Emoji.Should().Be("📝");
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateOnlyChangedFields()
    {
        // Arrange
        var existing = await SeedThemeAsync("Original", "🎨", "#000000");
        existing.Name = "Changed Name";

        // Act
        var result = await ThemeRepository.UpdateAsync(existing);

        // Assert
        result.Name.Should().Be("Changed Name");
        result.Emoji.Should().Be("🎨");
        result.Colors.Should().Be("#000000");
    }
}
