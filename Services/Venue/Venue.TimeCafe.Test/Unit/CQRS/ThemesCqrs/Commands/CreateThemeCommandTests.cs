namespace Venue.TimeCafe.Test.Unit.CQRS.ThemesCqrs.Commands;

public class CreateThemeCommandTests : BaseCqrsHandlerTest
{
    private readonly CreateThemeCommandHandler _handler;

    public CreateThemeCommandTests()
    {
        _handler = new CreateThemeCommandHandler(ThemeRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenThemeCreated()
    {
        var command = new CreateThemeCommand(TestData.NewThemes.NewTheme1Name, TestData.NewThemes.NewTheme1Emoji, TestData.NewThemes.NewTheme1Colors);
        var theme = new Theme { ThemeId = TestData.ExistingThemes.Theme1Id, Name = TestData.NewThemes.NewTheme1Name, Emoji = TestData.NewThemes.NewTheme1Emoji, Colors = TestData.NewThemes.NewTheme1Colors };

        ThemeRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Theme>())).ReturnsAsync(theme);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Theme.Should().NotBeNull();
        result.Theme!.Name.Should().Be(TestData.NewThemes.NewTheme1Name);
        result.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsNull()
    {
        var command = new CreateThemeCommand(TestData.NewThemes.NewTheme1Name, TestData.NewThemes.NewTheme1Emoji, TestData.NewThemes.NewTheme1Colors);

        ThemeRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Theme>()))!.ReturnsAsync((Theme?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreateThemeFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var command = new CreateThemeCommand(TestData.NewThemes.NewTheme1Name, TestData.NewThemes.NewTheme1Emoji, TestData.NewThemes.NewTheme1Colors);

        ThemeRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Theme>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("CreateThemeFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", "🎨", "#FF0000", false, "Название темы обязательно")]
    [InlineData(null, "🎨", "#FF0000", false, "Название темы обязательно")]
    [InlineData("A very long theme name that exceeds the maximum allowed length of one hundred characters for validation", "🎨", "#FF0000", false, "Название не может превышать 100 символов")]
    [InlineData("Valid Name", "🎨🎨🎨🎨🎨🎨", "#FF0000", false, "Эмодзи не может превышать 10 символов")]
    [InlineData("Valid Name", "🎨", "#FF0000", true, null)]
    [InlineData("Valid Name", null, null, true, null)]
    public async Task Validator_Should_ValidateCorrectly(string? name, string? emoji, string? colors, bool isValid, string? expectedError)
    {
        var command = new CreateThemeCommand(name!, emoji, colors);
        var validator = new CreateThemeCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
