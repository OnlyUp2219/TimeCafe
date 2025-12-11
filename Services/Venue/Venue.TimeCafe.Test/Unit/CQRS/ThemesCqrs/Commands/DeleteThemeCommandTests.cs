using Venue.TimeCafe.Test.Integration.Helpers;

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
        var command = new DeleteThemeCommand(TestData.ExistingThemes.Theme1Id.ToString());
        var theme = new Theme { ThemeId = TestData.ExistingThemes.Theme1Id, Name = TestData.ExistingThemes.Theme1Name };

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(TestData.ExistingThemes.Theme1Id)).ReturnsAsync(theme);
        ThemeRepositoryMock.Setup(r => r.DeleteAsync(TestData.ExistingThemes.Theme1Id)).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenThemeDoesNotExist()
    {
        var command = new DeleteThemeCommand(TestData.NonExistingIds.NonExistingThemeId.ToString());

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(TestData.NonExistingIds.NonExistingThemeId)).ReturnsAsync((Theme?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("ThemeNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsFalse()
    {
        var command = new DeleteThemeCommand(TestData.ExistingThemes.Theme1Id.ToString());
        var theme = new Theme { ThemeId = TestData.ExistingThemes.Theme1Id, Name = TestData.ExistingThemes.Theme1Name };

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(TestData.ExistingThemes.Theme1Id)).ReturnsAsync(theme);
        ThemeRepositoryMock.Setup(r => r.DeleteAsync(TestData.ExistingThemes.Theme1Id)).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeleteThemeFailed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var command = new DeleteThemeCommand(TestData.ExistingThemes.Theme1Id.ToString());

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(TestData.ExistingThemes.Theme1Id)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("DeleteThemeFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false, "Тема не найдена")]
    [InlineData("invalid-guid", false, "Тема не найдена")]
    [InlineData("00000000-0000-0000-0000-000000000000", false, "Тема не найдена")]
    [InlineData("a1111111-1111-1111-1111-111111111111", true, null)]
    public async Task Validator_Should_ValidateCorrectly(string themeId, bool isValid, string? expectedError)
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
