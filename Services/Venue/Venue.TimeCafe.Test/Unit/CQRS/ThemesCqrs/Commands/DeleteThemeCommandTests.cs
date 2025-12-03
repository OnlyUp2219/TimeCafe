namespace Venue.TimeCafe.Test.Unit.CQRS.ThemesCqrs.Commands;

public class DeleteThemeCommandTests : BaseCqrsHandlerTest
{
    private readonly DeleteThemeCommandHandler _handler;

    public DeleteThemeCommandTests()
    {
        _handler = new DeleteThemeCommandHandler(ThemeRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenThemeDeleted()
    {
        var command = new DeleteThemeCommand(1);
        var theme = new Theme { ThemeId = 1, Name = "Test Theme" };

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(theme);
        ThemeRepositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenThemeDoesNotExist()
    {
        var command = new DeleteThemeCommand(999);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Theme?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("ThemeNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsFalse()
    {
        var command = new DeleteThemeCommand(1);
        var theme = new Theme { ThemeId = 1, Name = "Test Theme" };

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(theme);
        ThemeRepositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeleteThemeFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var command = new DeleteThemeCommand(1);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeleteThemeFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(0, false, "ID темы обязателен")]
    [InlineData(-1, false, "ID темы обязателен")]
    [InlineData(1, true, null)]
    [InlineData(999, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int themeId, bool isValid, string? expectedError)
    {
        var command = new DeleteThemeCommand(themeId);
        var validator = new DeleteThemeCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
