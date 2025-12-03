namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class GetByBillingTypeAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetByBillingTypeAsync_Should_ReturnMatchingTariffs()
    {
        // Arrange
        await SeedTariffAsync("PerMinute1", 100m);
        await SeedTariffAsync("PerMinute2", 150m);

        var hourlyTariff = new Tariff
        {
            Name = "Hourly",
            PricePerMinute = 200m,
            BillingType = BillingType.Hourly,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        Context.Tariffs.Add(hourlyTariff);
        await Context.SaveChangesAsync();

        // Act
        var result = await TariffRepository.GetByBillingTypeAsync(BillingType.PerMinute);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.BillingType == BillingType.PerMinute);
    }

    [Fact]
    public async Task Repository_GetByBillingTypeAsync_Should_ReturnEmptyList_WhenNoMatches()
    {
        // Arrange
        await SeedTariffAsync("PerMinute", 100m);

        // Act
        var result = await TariffRepository.GetByBillingTypeAsync(BillingType.Hourly);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetByBillingTypeAsync_Should_ReturnOnlyActive()
    {
        // Arrange
        await SeedTariffAsync("Active", 100m);

        var inactiveTariff = new Tariff
        {
            Name = "Inactive",
            PricePerMinute = 150m,
            BillingType = BillingType.PerMinute,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        Context.Tariffs.Add(inactiveTariff);
        await Context.SaveChangesAsync();

        // Act
        var result = await TariffRepository.GetByBillingTypeAsync(BillingType.PerMinute);

        // Assert
        result.Should().HaveCount(1);
        result.Should().OnlyContain(t => t.IsActive);
    }

    [Fact]
    public async Task Repository_GetByBillingTypeAsync_Should_OrderByPricePerMinute()
    {
        // Arrange
        await SeedTariffAsync("Expensive", 300m);
        await SeedTariffAsync("Cheap", 100m);
        await SeedTariffAsync("Medium", 200m);

        // Act
        var result = (await TariffRepository.GetByBillingTypeAsync(BillingType.PerMinute)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].PricePerMinute.Should().Be(100m);
        result[1].PricePerMinute.Should().Be(200m);
        result[2].PricePerMinute.Should().Be(300m);
    }
}
