namespace Venue.TimeCafe.Test.Unit.CQRS.ThemesCqrs.Queries;

public class GetAllThemesQueryTests : BaseCqrsHandlerTest
{
    private readonly GetAllThemesQueryHandler _handler;

    public GetAllThemesQueryTests()
    {
        _handler = new GetAllThemesQueryHandler(ThemeRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenThemesFound()
    {
        var query = new GetAllThemesQuery();
        var themes = new List<Theme>
        {
            new() { ThemeId = 1, Name = "Theme 1" },
            new() { ThemeId = 2, Name = "Theme 2" }
        };

        ThemeRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(themes);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Themes.Should().NotBeNull();
        result.Themes.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoThemes()
    {
        var query = new GetAllThemesQuery();
        var themes = new List<Theme>();

        ThemeRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(themes);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Themes.Should().NotBeNull();
        result.Themes.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetAllThemesQuery();

        ThemeRepositoryMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetThemesFailed");
        result.StatusCode.Should().Be(500);
    }
}
