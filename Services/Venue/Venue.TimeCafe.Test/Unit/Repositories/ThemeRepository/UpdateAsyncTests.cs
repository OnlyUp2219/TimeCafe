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
        var existing = await SeedThemeAsync(TestData.ExistingThemes.Theme1Name);
        existing.Name = TestData.ExistingThemes.Theme2Name;
        existing.Emoji = TestData.ExistingThemes.Theme2Emoji;
        existing.Colors = TestData.ExistingThemes.Theme2Colors;

        // Act
        var result = await ThemeRepository.UpdateAsync(existing);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(TestData.ExistingThemes.Theme2Name);
        result.Emoji.Should().Be(TestData.ExistingThemes.Theme2Emoji);
        result.Colors.Should().Be(TestData.ExistingThemes.Theme2Colors);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistent = new Theme
        {
            ThemeId = TestData.NonExistingIds.NonExistingThemeId,
            Name = TestData.DefaultValues.DefaultThemeName
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
        var existing = await SeedThemeAsync(TestData.ExistingThemes.Theme1Name);
        existing.Name = TestData.ExistingThemes.Theme2Name;

        // Act
        await ThemeRepository.UpdateAsync(existing);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_PersistChanges()
    {
        // Arrange
        var existing = await SeedThemeAsync(TestData.ExistingThemes.Theme1Name);
        existing.Name = TestData.ExistingThemes.Theme3Name;
        existing.Emoji = TestData.ExistingThemes.Theme3Emoji;

        // Act
        await ThemeRepository.UpdateAsync(existing);

        // Assert
        var fromDb = await Context.Themes.FindAsync(existing.ThemeId);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be(TestData.ExistingThemes.Theme3Name);
        fromDb.Emoji.Should().Be(TestData.ExistingThemes.Theme3Emoji);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateOnlyChangedFields()
    {
        // Arrange
        var existing = await SeedThemeAsync(TestData.ExistingThemes.Theme1Name, TestData.ExistingThemes.Theme1Emoji, TestData.ExistingThemes.Theme1Colors);
        existing.Name = TestData.ExistingThemes.Theme2Name;

        // Act
        var result = await ThemeRepository.UpdateAsync(existing);

        // Assert
        result.Name.Should().Be(TestData.ExistingThemes.Theme2Name);
        result.Emoji.Should().Be(TestData.ExistingThemes.Theme1Emoji);
        result.Colors.Should().Be(TestData.ExistingThemes.Theme1Colors);
    }
}
