namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

public class HasActiveVisitAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_HasActiveVisitAsync_Should_ReturnTrue_WhenUserHasActiveVisit()
    {
        // Arrange
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);

        // Act
        var result = await VisitRepository.HasActiveVisitAsync(TestData.ExistingVisits.Visit1UserId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_HasActiveVisitAsync_Should_ReturnFalse_WhenUserHasNoActiveVisit()
    {
        // Arrange
        var visit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        visit.Status = VisitStatus.Completed;
        await Context.SaveChangesAsync();

        // Act
        var result = await VisitRepository.HasActiveVisitAsync(TestData.ExistingVisits.Visit1UserId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_HasActiveVisitAsync_Should_ReturnFalse_WhenUserNotExists()
    {
        // Act
        var result = await VisitRepository.HasActiveVisitAsync(TestData.NonExistingIds.NonExistingUserId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_HasActiveVisitAsync_Should_ReturnTrue_OnlyForActiveStatus()
    {
        // Arrange
        var completedVisit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        completedVisit.Status = VisitStatus.Completed;
        await Context.SaveChangesAsync();

        var activeVisit = await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);

        // Act
        var result = await VisitRepository.HasActiveVisitAsync(TestData.ExistingVisits.Visit1UserId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_HasActiveVisitAsync_Should_BeUserSpecific()
    {
        // Arrange
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId);
        await SeedVisitAsync(TestData.ExistingVisits.Visit2UserId);

        // Act
        var resultUser1 = await VisitRepository.HasActiveVisitAsync(TestData.ExistingVisits.Visit1UserId);
        var resultUser2 = await VisitRepository.HasActiveVisitAsync(TestData.ExistingVisits.Visit2UserId);
        var resultNonexistent = await VisitRepository.HasActiveVisitAsync(TestData.NonExistingIds.NonExistingUserId);

        // Assert
        resultUser1.Should().BeTrue();
        resultUser2.Should().BeTrue();
        resultNonexistent.Should().BeFalse();
    }
}
