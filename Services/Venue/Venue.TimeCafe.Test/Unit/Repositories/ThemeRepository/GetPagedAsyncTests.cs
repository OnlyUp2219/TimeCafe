namespace Venue.TimeCafe.Test.Unit.Repositories.ThemeRepository;

public class GetPagedAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetPagedAsync_Should_ReturnPagedThemes()
    {
        // Arrange
        await SeedThemeAsync("Theme A");
        await SeedThemeAsync("Theme B");
        await SeedThemeAsync("Theme C");

        // Act
        var result = (await ThemeRepository.GetPagedAsync(1, 2)).ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Theme A");
        result[1].Name.Should().Be("Theme B");
    }

    [Fact]
    public async Task Repository_GetPagedAsync_Should_ReturnEmpty_WhenPageOutOfBounds()
    {
        // Arrange
        await SeedThemeAsync("Theme A");

        // Act
        var result = await ThemeRepository.GetPagedAsync(2, 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetPagedAsync_Should_ReturnFromCache_WhenCached()
    {
        // Arrange
        var theme = await SeedThemeAsync("Theme A");

        // First call fills cache
        await ThemeRepository.GetPagedAsync(1, 10);

        // Modify DB directly
        var dbTheme = await Context.Themes.FindAsync(theme.ThemeId);
        dbTheme!.Name = "Updated Name";
        await Context.SaveChangesAsync();

        // Act
        var result = (await ThemeRepository.GetPagedAsync(1, 10)).ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Theme A"); // Should be old name from cache
    }
}
