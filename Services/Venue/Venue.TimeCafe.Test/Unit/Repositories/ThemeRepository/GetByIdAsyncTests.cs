namespace Venue.TimeCafe.Test.Unit.Repositories.ThemeRepository;

public class GetByIdAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnTheme_WhenExists()
    {
        // Arrange
        var theme = await SeedThemeAsync(TestData.ExistingThemes.Theme1Name);

        // Act
        var result = await ThemeRepository.GetByIdAsync(theme.ThemeId);

        // Assert
        result.Should().NotBeNull();
        result!.ThemeId.Should().Be(theme.ThemeId);
        result.Name.Should().Be(TestData.ExistingThemes.Theme1Name);
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = TestData.NonExistingIds.NonExistingThemeId;

        // Act
        var result = await ThemeRepository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        var theme = await SeedThemeAsync(TestData.ExistingThemes.Theme1Name);
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
            Name = TestData.ExistingThemes.Theme2Name,
            Emoji = TestData.ExistingThemes.Theme2Emoji,
            Colors = TestData.ExistingThemes.Theme2Colors
        };
        Context.Themes.Add(theme);
        await Context.SaveChangesAsync();

        // Act
        var result = await ThemeRepository.GetByIdAsync(theme.ThemeId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(TestData.ExistingThemes.Theme2Name);
        result.Emoji.Should().Be(TestData.ExistingThemes.Theme2Emoji);
        result.Colors.Should().Be(TestData.ExistingThemes.Theme2Colors);
    }
}
