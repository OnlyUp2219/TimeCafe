namespace Venue.TimeCafe.Test.Unit.Repositories.ThemeRepository;

public class DeleteAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_DeleteAsync_Should_ReturnTrue_WhenThemeExists()
    {
        // Arrange
        var theme = await SeedThemeAsync("To Delete");

        // Act
        var result = await ThemeRepository.DeleteAsync(theme.ThemeId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_ReturnFalse_WhenThemeNotExists()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await ThemeRepository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_RemoveFromDatabase()
    {
        // Arrange
        var theme = await SeedThemeAsync("To Remove");
        var themeId = theme.ThemeId;

        // Act
        await ThemeRepository.DeleteAsync(themeId);

        // Assert
        var fromDb = await Context.Themes.FindAsync(themeId);
        fromDb.Should().BeNull();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_InvalidateCache()
    {
        // Arrange
        var theme = await SeedThemeAsync("Cache Test");

        // Act
        await ThemeRepository.DeleteAsync(theme.ThemeId);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_HandleAlreadyDeleted()
    {
        // Arrange
        var theme = await SeedThemeAsync("To Delete Twice");
        var themeId = theme.ThemeId;

        // Act
        var firstDelete = await ThemeRepository.DeleteAsync(themeId);
        var secondDelete = await ThemeRepository.DeleteAsync(themeId);

        // Assert
        firstDelete.Should().BeTrue();
        secondDelete.Should().BeFalse();
    }
}
