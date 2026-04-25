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
        var command = new DeleteThemeCommand(TestData.ExistingThemes.Theme1Id);
        var theme = new Theme { ThemeId = TestData.ExistingThemes.Theme1Id, Name = TestData.ExistingThemes.Theme1Name };

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == TestData.ExistingThemes.Theme1Id), It.IsAny<CancellationToken>())).ReturnsAsync(theme);
        ThemeRepositoryMock.Setup(r => r.DeleteAsync(It.Is<Guid>(id => id == TestData.ExistingThemes.Theme1Id), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenThemeDoesNotExist()
    {
        var command = new DeleteThemeCommand(TestData.NonExistingIds.NonExistingThemeId);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == TestData.NonExistingIds.NonExistingThemeId), It.IsAny<CancellationToken>())).ReturnsAsync((Theme?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenRepositoryReturnsFalse()
    {
        var command = new DeleteThemeCommand(TestData.ExistingThemes.Theme1Id);
        var theme = new Theme { ThemeId = TestData.ExistingThemes.Theme1Id, Name = TestData.ExistingThemes.Theme1Name };

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == TestData.ExistingThemes.Theme1Id), It.IsAny<CancellationToken>())).ReturnsAsync(theme);
        ThemeRepositoryMock.Setup(r => r.DeleteAsync(It.Is<Guid>(id => id == TestData.ExistingThemes.Theme1Id), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var command = new DeleteThemeCommand(TestData.ExistingThemes.Theme1Id);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == TestData.ExistingThemes.Theme1Id), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", false, "Тема не найдена")]
    [InlineData("a1111111-1111-1111-1111-111111111111", true, null)]
    public async Task Validator_Should_ValidateCorrectly(string themeIdStr, bool isValid, string? expectedError)
    {
        var command = new DeleteThemeCommand(Guid.Parse(themeIdStr));
        var validator = new DeleteThemeCommandValidator();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}

