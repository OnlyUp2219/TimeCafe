namespace Venue.TimeCafe.Test.Unit.CQRS.ThemesCqrs.Queries;

public class GetThemeByIdQueryTests : BaseCqrsHandlerTest
{
    private readonly GetThemeByIdQueryHandler _handler;

    public GetThemeByIdQueryTests()
    {
        _handler = new GetThemeByIdQueryHandler(UowMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenThemeFound()
    {
        var query = new GetThemeByIdQuery(TestData.ExistingThemes.Theme1Id);
        var theme = new Theme { ThemeId = TestData.ExistingThemes.Theme1Id, Name = TestData.ExistingThemes.Theme1Name, Emoji = TestData.ExistingThemes.Theme1Emoji, Colors = TestData.ExistingThemes.Theme1Colors };

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == TestData.ExistingThemes.Theme1Id), It.IsAny<CancellationToken>())).ReturnsAsync(theme);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(TestData.ExistingThemes.Theme1Name);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenThemeDoesNotExist()
    {
        var query = new GetThemeByIdQuery(TestData.NonExistingIds.NonExistingThemeId);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == TestData.NonExistingIds.NonExistingThemeId), It.IsAny<CancellationToken>())).ReturnsAsync((Theme?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var query = new GetThemeByIdQuery(TestData.ExistingThemes.Theme1Id);

        ThemeRepositoryMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == TestData.ExistingThemes.Theme1Id), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
    }

}

