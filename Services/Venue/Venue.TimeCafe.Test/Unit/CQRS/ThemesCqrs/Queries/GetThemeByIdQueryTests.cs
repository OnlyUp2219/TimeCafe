using Venue.TimeCafe.Test.Integration.Helpers;

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
        var query = new GetThemeByIdQuery(TestData.ExistingThemes.Theme1Id.ToString());
        var theme = new Theme { ThemeId = TestData.ExistingThemes.Theme1Id, Name = TestData.ExistingThemes.Theme1Name, Emoji = TestData.ExistingThemes.Theme1Emoji, Colors = TestData.ExistingThemes.Theme1Colors };

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(TestData.ExistingThemes.Theme1Id)).ReturnsAsync(theme);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Theme.Should().NotBeNull();
        result.Theme!.Name.Should().Be(TestData.ExistingThemes.Theme1Name);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenThemeDoesNotExist()
    {
        var query = new GetThemeByIdQuery(TestData.NonExistingIds.NonExistingThemeId.ToString());

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(TestData.NonExistingIds.NonExistingThemeId)).ReturnsAsync((Theme?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("ThemeNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetThemeByIdQuery(TestData.ExistingThemes.Theme1Id.ToString());

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(TestData.ExistingThemes.Theme1Id)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetThemeFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false, "Тема не найдена")]
    [InlineData("invalid-guid", false, "Тема не найдена")]
    [InlineData("00000000-0000-0000-0000-000000000000", false, "Тема не найдена")]
    [InlineData("a1111111-1111-1111-1111-111111111111", true, null)]
    public async Task Validator_Should_ValidateCorrectly(string themeId, bool isValid, string? expectedError)
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
