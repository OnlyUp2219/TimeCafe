namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

public class GetTotalCountAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_ReturnCorrectCount()
    {
        // Arrange
        await SeedVisitAsync();
        await SeedVisitAsync();

        // Act
        var result = await VisitRepository.GetTotalCountAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_ReturnZero_WhenNoVisits()
    {
        // Act
        var result = await VisitRepository.GetTotalCountAsync();

        // Assert
        result.Should().Be(0);
    }
}
