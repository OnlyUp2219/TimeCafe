namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

public class HasActiveVisitAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_HasActiveVisitAsync_Should_ReturnTrue_WhenUserHasActiveVisit()
    {
        // Arrange
        await SeedVisitAsync("user123");

        // Act
        var result = await VisitRepository.HasActiveVisitAsync("user123");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_HasActiveVisitAsync_Should_ReturnFalse_WhenUserHasNoActiveVisit()
    {
        // Arrange
        var visit = await SeedVisitAsync("user123");
        visit.Status = VisitStatus.Completed;
        await Context.SaveChangesAsync();

        // Act
        var result = await VisitRepository.HasActiveVisitAsync("user123");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_HasActiveVisitAsync_Should_ReturnFalse_WhenUserNotExists()
    {
        // Act
        var result = await VisitRepository.HasActiveVisitAsync("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_HasActiveVisitAsync_Should_ReturnTrue_OnlyForActiveStatus()
    {
        // Arrange
        var completedVisit = await SeedVisitAsync("user123");
        completedVisit.Status = VisitStatus.Completed;
        await Context.SaveChangesAsync();

        var activeVisit = await SeedVisitAsync("user123");

        // Act
        var result = await VisitRepository.HasActiveVisitAsync("user123");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_HasActiveVisitAsync_Should_BeUserSpecific()
    {
        // Arrange
        await SeedVisitAsync("user123");
        await SeedVisitAsync("other_user");

        // Act
        var resultUser123 = await VisitRepository.HasActiveVisitAsync("user123");
        var resultOther = await VisitRepository.HasActiveVisitAsync("other_user");
        var resultNonexistent = await VisitRepository.HasActiveVisitAsync("nonexistent");

        // Assert
        resultUser123.Should().BeTrue();
        resultOther.Should().BeTrue();
        resultNonexistent.Should().BeFalse();
    }
}
