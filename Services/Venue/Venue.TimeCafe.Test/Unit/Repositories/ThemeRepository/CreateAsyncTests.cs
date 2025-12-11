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
            Name = TestData.NewThemes.NewTheme1Name,
            Emoji = TestData.NewThemes.NewTheme1Emoji,
            Colors = TestData.NewThemes.NewTheme1Colors
        };

        // Act
        var result = await ThemeRepository.CreateAsync(theme);

        // Assert
        result.Should().NotBeNull();
        result.ThemeId.Should().NotBe(Guid.Empty);
        result.Name.Should().Be(TestData.NewThemes.NewTheme1Name);
        result.Emoji.Should().Be(TestData.NewThemes.NewTheme1Emoji);
        result.Colors.Should().Be(TestData.NewThemes.NewTheme1Colors);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_PersistToDatabase()
    {
        // Arrange
        var theme = new Theme
        {
            Name = TestData.NewThemes.NewTheme2Name,
            Emoji = TestData.NewThemes.NewTheme2Emoji
        };

        // Act
        var result = await ThemeRepository.CreateAsync(theme);

        // Assert
        var fromDb = await Context.Themes.FindAsync(result.ThemeId);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be(TestData.NewThemes.NewTheme2Name);
        fromDb.Emoji.Should().Be(TestData.NewThemes.NewTheme2Emoji);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_InvalidateCache()
    {
        // Arrange
        var theme = new Theme { Name = TestData.DefaultValues.DefaultThemeName };

        // Act
        await ThemeRepository.CreateAsync(theme);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_CreateThemeWithMinimalData()
    {
        // Arrange
        var theme = new Theme { Name = TestData.DefaultValues.DefaultThemeName };

        // Act
        var result = await ThemeRepository.CreateAsync(theme);

        // Assert
        result.Should().NotBeNull();
        result.ThemeId.Should().NotBe(Guid.Empty);
        result.Name.Should().Be(TestData.DefaultValues.DefaultThemeName);
        result.Emoji.Should().BeNull();
        result.Colors.Should().BeNull();
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_CreateThemeWithAllProperties()
    {
        // Arrange
        var theme = new Theme
        {
            Name = TestData.ExistingThemes.Theme1Name,
            Emoji = TestData.ExistingThemes.Theme1Emoji,
            Colors = TestData.ExistingThemes.Theme1Colors
        };

        // Act
        var result = await ThemeRepository.CreateAsync(theme);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(TestData.ExistingThemes.Theme1Name);
        result.Emoji.Should().Be(TestData.ExistingThemes.Theme1Emoji);
        result.Colors.Should().Be(TestData.ExistingThemes.Theme1Colors);
    }
}
