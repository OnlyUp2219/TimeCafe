namespace Venue.TimeCafe.Test.Unit.Repositories.VisitRepository;

using Venue.TimeCafe.Test.Integration.Helpers;

public class CreateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_CreateAsync_Should_ThrowException_WhenVisitIsNull()
    {
        // Arrange
        Visit? nullVisit = null;

        // Act
        var act = async () => await VisitRepository.CreateAsync(nullVisit!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_CreateVisit_WhenValidData()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);
        var visit = new Visit
        {
            UserId = TestData.ExistingVisits.Visit1UserId,
            TariffId = tariff.TariffId
        };

        // Act
        var result = await VisitRepository.CreateAsync(visit);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(TestData.ExistingVisits.Visit1UserId);
        result.TariffId.Should().Be(tariff.TariffId);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_SetEntryTime()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);
        var visit = new Visit
        {
            UserId = TestData.ExistingVisits.Visit1UserId,
            TariffId = tariff.TariffId
        };

        // Act
        var result = await VisitRepository.CreateAsync(visit);

        // Assert
        result.EntryTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_SetStatusToActive()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);
        var visit = new Visit
        {
            UserId = TestData.ExistingVisits.Visit1UserId,
            TariffId = tariff.TariffId
        };

        // Act
        var result = await VisitRepository.CreateAsync(visit);

        // Assert
        result.Status.Should().Be(VisitStatus.Active);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_PersistToDatabase()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);
        var visit = new Visit
        {
            UserId = TestData.NewVisits.NewVisit1UserId,
            TariffId = tariff.TariffId
        };

        // Act
        var result = await VisitRepository.CreateAsync(visit);

        // Assert
        var fromDb = await Context.Visits.FindAsync(result.VisitId);
        fromDb.Should().NotBeNull();
        fromDb!.UserId.Should().Be(TestData.NewVisits.NewVisit1UserId);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_InvalidateCache()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);
        var visit = new Visit
        {
            UserId = TestData.ExistingVisits.Visit1UserId,
            TariffId = tariff.TariffId
        };

        // Act
        await VisitRepository.CreateAsync(visit);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
