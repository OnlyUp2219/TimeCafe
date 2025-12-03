namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class GetTotalCountAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_ReturnCorrectCount()
    {
        // Arrange
        await SeedTariffAsync("Tariff 1", 100m);
        await SeedTariffAsync("Tariff 2", 200m);
        await SeedTariffAsync("Tariff 3", 300m);

        // Act
        var result = await TariffRepository.GetTotalCountAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_ReturnZero_WhenNoTariffs()
    {
        // Act
        var result = await TariffRepository.GetTotalCountAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_CountBothActiveAndInactive()
    {
        // Arrange
        await SeedTariffAsync("Active", 100m);

        var inactiveTariff = new Tariff
        {
            Name = "Inactive",
            PricePerMinute = 200m,
            BillingType = BillingType.PerMinute,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        Context.Tariffs.Add(inactiveTariff);
        await Context.SaveChangesAsync();

        // Act
        var result = await TariffRepository.GetTotalCountAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_UpdateAfterCreate()
    {
        // Arrange
        await SeedTariffAsync("Initial", 100m);
        var countBefore = await TariffRepository.GetTotalCountAsync();

        // Act
        await SeedTariffAsync("New", 200m);
        var countAfter = await TariffRepository.GetTotalCountAsync();

        // Assert
        countBefore.Should().Be(1);
        countAfter.Should().Be(2);
    }

    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_UpdateAfterDelete()
    {
        // Arrange
        var tariff1 = await SeedTariffAsync("To Keep", 100m);
        var tariff2 = await SeedTariffAsync("To Delete", 200m);
        var countBefore = await TariffRepository.GetTotalCountAsync();

        // Act
        await TariffRepository.DeleteAsync(tariff2.TariffId);
        var countAfter = await TariffRepository.GetTotalCountAsync();

        // Assert
        countBefore.Should().Be(2);
        countAfter.Should().Be(1);
    }
}
