namespace Venue.TimeCafe.Test.Unit.Repositories.ThemeRepository;

public class GetAllAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnAllThemes()
    {
        // Arrange
        await SeedThemeAsync("Theme 1");
        await SeedThemeAsync("Theme 2");
        await SeedThemeAsync("Theme 3");

        // Act
        var result = await ThemeRepository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnEmptyList_WhenNoThemes()
    {
        // Act
        var result = await ThemeRepository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnOrderedByName()
    {
        // Arrange
        await SeedThemeAsync("Zebra");
        await SeedThemeAsync("Alpha");
        await SeedThemeAsync("Beta");

        // Act
        var result = (await ThemeRepository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alpha");
        result[1].Name.Should().Be("Beta");
        result[2].Name.Should().Be("Zebra");
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        await SeedThemeAsync("Theme1");
        await SeedThemeAsync("Theme2");
        await ThemeRepository.GetAllAsync();
        await ThemeRepository.GetAllAsync();

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnThemesWithAllProperties()
    {
        // Arrange
        var theme = new Theme
        {
            Name = "Rich Theme",
            Emoji = "🌟",
            Colors = "#FFFFFF,#000000"
        };
        Context.Themes.Add(theme);
        await Context.SaveChangesAsync();

        // Act
        var result = (await ThemeRepository.GetAllAsync()).First();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Rich Theme");
        result.Emoji.Should().Be("🌟");
        result.Colors.Should().Be("#FFFFFF,#000000");
    }
}
