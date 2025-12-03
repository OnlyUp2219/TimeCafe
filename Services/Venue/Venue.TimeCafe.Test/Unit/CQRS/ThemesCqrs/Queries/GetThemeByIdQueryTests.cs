namespace Venue.TimeCafe.Test.Unit.CQRS.ThemesCqrs.Queries;

public class GetThemeByIdQueryTests : BaseCqrsHandlerTest
{
    private readonly GetThemeByIdQueryHandler _handler;

    public GetThemeByIdQueryTests()
    {
        _handler = new GetThemeByIdQueryHandler(ThemeRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenThemeFound()
    {
        var query = new GetThemeByIdQuery(1);
        var theme = new Theme { ThemeId = 1, Name = "Test Theme", Emoji = "🎨", Colors = "#FF0000" };

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(theme);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Theme.Should().NotBeNull();
        result.Theme!.Name.Should().Be("Test Theme");
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenThemeDoesNotExist()
    {
        var query = new GetThemeByIdQuery(999);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Theme?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("ThemeNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetThemeByIdQuery(1);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetThemeFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(0, false, "ID темы обязателен")]
    [InlineData(-1, false, "ID темы обязателен")]
    [InlineData(1, true, null)]
    [InlineData(999, true, null)]
    public async Task Validator_Should_ValidateCorrectly(int themeId, bool isValid, string? expectedError)
    {
        var query = new GetThemeByIdQuery(themeId);
        var validator = new GetThemeByIdQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().Be(isValid);
        if (!isValid)
        {
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError!));
        }
    }
}
