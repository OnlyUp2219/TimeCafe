namespace Venue.TimeCafe.Test.Unit.CQRS.ThemesCqrs.Events;

public class ThemeChangedEventHandlerTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_Should_InvalidateThemesCache()
    {
        // Arrange
        var theme = await SeedThemeAsync("Theme 1");
        var handler = new ThemeChangedEventHandler(HybridCache);

        // Fill cache
        await ThemeRepository.GetPagedAsync(1, 10);

        // Modify DB directly
        var dbTheme = await Context.Themes.FindAsync(theme.ThemeId);
        dbTheme!.Name = "Updated Name";
        await Context.SaveChangesAsync();

        // Act
        await handler.Handle(new ThemeChangedEvent(theme.ThemeId));

        // Assert
        var result = (await ThemeRepository.GetPagedAsync(1, 10)).ToList();
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Updated Name"); // Should be NEW name because cache was invalidated
    }
}
