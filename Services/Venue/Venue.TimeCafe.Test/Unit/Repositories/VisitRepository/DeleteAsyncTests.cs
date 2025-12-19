namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

public class DeleteAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_DeleteAsync_Should_ReturnTrue_WhenVisitExists()
    {
        // Arrange
        var visit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);

        // Act
        var result = await VisitRepository.DeleteAsync(visit.VisitId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_ReturnFalse_WhenVisitNotExists()
    {
        // Arrange
        var nonExistentId = TestData.NonExistingIds.NonExistingVisitId;

        // Act
        var result = await VisitRepository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_RemoveFromDatabase()
    {
        // Arrange
        var visit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        var visitId = visit.VisitId;

        // Act
        await VisitRepository.DeleteAsync(visitId);

        // Assert
        var fromDb = await Context.Visits.FindAsync(visitId);
        fromDb.Should().BeNull();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_InvalidateCache()
    {
        // Arrange
        var visit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);

        // Act
        await VisitRepository.DeleteAsync(visit.VisitId);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_HandleAlreadyDeleted()
    {
        // Arrange
        var visit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        var visitId = visit.VisitId;

        // Act
        var firstDelete = await VisitRepository.DeleteAsync(visitId);
        var secondDelete = await VisitRepository.DeleteAsync(visitId);

        // Assert
        firstDelete.Should().BeTrue();
        secondDelete.Should().BeFalse();
    }
}
