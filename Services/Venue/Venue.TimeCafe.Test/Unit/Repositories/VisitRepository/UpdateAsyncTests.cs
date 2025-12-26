namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

public class UpdateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_UpdateAsync_Should_ThrowException_WhenVisitIsNull()
    {
        // Arrange
        Visit? nullVisit = null;

        // Act
        var act = async () => await VisitRepository.UpdateAsync(nullVisit!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateVisit_WhenExists()
    {
        // Arrange
        var existing = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        existing.Status = VisitStatus.Completed;
        existing.ExitTime = DateTimeOffset.UtcNow;
        existing.CalculatedCost = TestData.VisitUpdateData.UpdatedCalculatedCost;

        // Act
        var result = await VisitRepository.UpdateAsync(existing);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(VisitStatus.Completed);
        result.ExitTime.Should().NotBeNull();
        result.CalculatedCost.Should().Be(TestData.VisitUpdateData.UpdatedCalculatedCost);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);
        var nonExistent = new Visit
        {
            VisitId = TestData.NonExistingIds.NonExistingVisitId,
            UserId = TestData.ExistingVisits.Visit1UserId,
            TariffId = tariff.TariffId
        };

        // Act
        var result = await VisitRepository.UpdateAsync(nonExistent);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_InvalidateCache()
    {
        // Arrange
        var existing = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        existing.Status = VisitStatus.Completed;

        // Act
        await VisitRepository.UpdateAsync(existing);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_PersistChanges()
    {
        // Arrange
        var existing = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        existing.Status = VisitStatus.Completed;
        existing.ExitTime = DateTimeOffset.UtcNow;
        existing.CalculatedCost = TestData.VisitUpdateData.UpdatedCalculatedCost;

        // Act
        await VisitRepository.UpdateAsync(existing);

        // Assert
        var fromDb = await Context.Visits.FindAsync(existing.VisitId);
        fromDb.Should().NotBeNull();
        fromDb!.Status.Should().Be(VisitStatus.Completed);
        fromDb.ExitTime.Should().NotBeNull();
        fromDb.CalculatedCost.Should().Be(TestData.VisitUpdateData.UpdatedCalculatedCost);
    }
}
