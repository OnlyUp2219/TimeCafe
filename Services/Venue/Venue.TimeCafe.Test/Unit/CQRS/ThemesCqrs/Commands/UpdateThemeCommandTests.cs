namespace Venue.TimeCafe.Test.Unit.CQRS.ThemesCqrs.Commands;

public class UpdateThemeCommandTests : BaseCqrsHandlerTest
{
    private readonly UpdateThemeCommandHandler _handler;

    public UpdateThemeCommandTests()
    {
        _handler = new UpdateThemeCommandHandler(ThemeRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenThemeUpdated()
    {
        var theme = new Theme { ThemeId = 1, Name = "Updated Theme", Emoji = "🎨", Colors = "#FF0000" };
        var command = new UpdateThemeCommand(theme);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(theme);
        ThemeRepositoryMock.Setup(r => r.UpdateAsync(theme)).ReturnsAsync(theme);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Theme.Should().NotBeNull();
        result.Theme!.Name.Should().Be("Updated Theme");
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenThemeDoesNotExist()
    {
        var theme = new Theme { ThemeId = 999, Name = "Nonexistent", Emoji = "🎨", Colors = "#FF0000" };
        var command = new UpdateThemeCommand(theme);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Theme?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("ThemeNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsNull()
    {
        var theme = new Theme { ThemeId = 1, Name = "Updated Theme", Emoji = "🎨", Colors = "#FF0000" };
        var command = new UpdateThemeCommand(theme);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(theme);
        ThemeRepositoryMock.Setup(r => r.UpdateAsync(theme)).ReturnsAsync((Theme?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("UpdateThemeFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var theme = new Theme { ThemeId = 1, Name = "Updated Theme", Emoji = "🎨", Colors = "#FF0000" };
        var command = new UpdateThemeCommand(theme);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("UpdateThemeFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(null, "Test", "🎨", "#FF0000", false, "Тема обязательна")]
    [InlineData(0, "Test", "🎨", "#FF0000", false, "ID темы обязателен")]
    [InlineData(-1, "Test", "🎨", "#FF0000", false, "ID темы обязателен")]
    [InlineData(1, "", "🎨", "#FF0000", false, "Название темы обязательно")]
    [InlineData(1, null, "🎨", "#FF0000", false, "Название темы обязательно")]
    [InlineData(1, "A very long theme name that exceeds the maximum allowed length of one hundred characters for validation", "🎨", "#FF0000", false, "Название не может превышать 100 символов")]
    [InlineData(1, "Valid Name", "🎨🎨🎨🎨🎨🎨", "#FF0000", false, "Эмодзи не может превышать 10 символов")]
    [InlineData(1, "Valid Name", "🎨", "#FF0000", true, null)]
    [InlineData(1, "Valid Name", null, null, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int? themeId, string? name, string? emoji, string? colors, bool isValid, string? expectedError)
    {
        var theme = themeId.HasValue ? new Theme { ThemeId = themeId.Value, Name = name!, Emoji = emoji, Colors = colors } : null;
        var command = new UpdateThemeCommand(theme!);
        var validator = new UpdateThemeCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
