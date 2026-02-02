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
        var theme = new Theme { ThemeId = TestData.ExistingThemes.Theme1Id, Name = TestData.ExistingThemes.Theme2Name, Emoji = TestData.ExistingThemes.Theme2Emoji, Colors = TestData.ExistingThemes.Theme2Colors };
        var command = new UpdateThemeCommand(TestData.ExistingThemes.Theme1Id.ToString(), TestData.ExistingThemes.Theme2Name, TestData.ExistingThemes.Theme2Emoji, TestData.ExistingThemes.Theme2Colors);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == TestData.ExistingThemes.Theme1Id))).ReturnsAsync(theme);
        ThemeRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Theme>())).ReturnsAsync(theme);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Theme.Should().NotBeNull();
        result.Theme!.Name.Should().Be(TestData.ExistingThemes.Theme2Name);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenThemeDoesNotExist()
    {
        var command = new UpdateThemeCommand(TestData.NonExistingIds.NonExistingThemeId.ToString(), TestData.ExistingThemes.Theme1Name, TestData.ExistingThemes.Theme1Emoji, TestData.ExistingThemes.Theme1Colors);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == TestData.NonExistingIds.NonExistingThemeId))).ReturnsAsync((Theme?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("ThemeNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ThrowCqrsResultException_WhenRepositoryThrowsException()
    {
        var theme = new Theme { ThemeId = TestData.ExistingThemes.Theme1Id, Name = TestData.ExistingThemes.Theme2Name, Emoji = TestData.ExistingThemes.Theme2Emoji, Colors = TestData.ExistingThemes.Theme2Colors };
        var command = new UpdateThemeCommand(TestData.ExistingThemes.Theme1Id.ToString(), TestData.ExistingThemes.Theme2Name, TestData.ExistingThemes.Theme2Emoji, TestData.ExistingThemes.Theme2Colors);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == TestData.ExistingThemes.Theme1Id))).ReturnsAsync(theme);
        ThemeRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Theme>())).ThrowsAsync(new Exception());

        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("UpdateThemeFailed");
        ex.Result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ThrowCqrsResultException_WhenExceptionThrown()
    {
        var command = new UpdateThemeCommand(TestData.ExistingThemes.Theme1Id.ToString(), TestData.ExistingThemes.Theme2Name, TestData.ExistingThemes.Theme2Emoji, TestData.ExistingThemes.Theme2Colors);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == TestData.ExistingThemes.Theme1Id))).ThrowsAsync(new Exception());

        var ex = await Assert.ThrowsAsync<CqrsResultException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.Result.Should().NotBeNull();
        ex.Result!.Success.Should().BeFalse();
        ex.Result.Code.Should().Be("UpdateThemeFailed");
        ex.Result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", "Test", "🎨", "#FF0000", false, "Тема не найдена")]
    [InlineData("invalid-guid", "Test", "🎨", "#FF0000", false, "Тема не найдена")]
    [InlineData("a1111111-1111-1111-1111-111111111111", "", "🎨", "#FF0000", false, "Название темы обязательно")]
    [InlineData("a1111111-1111-1111-1111-111111111111", null, "🎨", "#FF0000", false, "Название темы обязательно")]
    [InlineData("a1111111-1111-1111-1111-111111111111", "A very long theme name that exceeds the maximum allowed length of one hundred characters for validation", "🎨", "#FF0000", false, "Название не может превышать 100 символов")]
    [InlineData("a1111111-1111-1111-1111-111111111111", "Valid Name", "🎨🎨🎨🎨🎨🎨", "#FF0000", false, "Эмодзи не может превышать 10 символов")]
    [InlineData("a1111111-1111-1111-1111-111111111111", "Valid Name", "🎨", "{\"primary\":\"#FF0000\"}", true, null)]
    [InlineData("a1111111-1111-1111-1111-111111111111", "Valid Name", null, null, true, null)]
    public async Task Validator_Should_ValidateCorrectly(string themeId, string? name, string? emoji, string? colors, bool isValid, string? expectedError)
    {
        var command = new UpdateThemeCommand(themeId, name!, emoji, colors);
        var validator = new UpdateThemeCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
