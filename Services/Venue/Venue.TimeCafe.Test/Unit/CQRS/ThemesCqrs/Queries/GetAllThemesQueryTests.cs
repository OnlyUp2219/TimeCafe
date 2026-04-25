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
            new() { ThemeId = TestData.ExistingThemes.Theme1Id, Name = TestData.ExistingThemes.Theme1Name },
            new() { ThemeId = TestData.ExistingThemes.Theme2Id, Name = TestData.ExistingThemes.Theme2Name }
        };

        ThemeRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(themes);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenNoThemes()
    {
        var query = new GetAllThemesQuery();
        var themes = new List<Theme>();

        ThemeRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(themes);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetAllThemesQuery();

        ThemeRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }
}

