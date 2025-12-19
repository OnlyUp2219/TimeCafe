namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class GetByBillingTypeAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetByBillingTypeAsync_Should_ReturnMatchingTariffs()
    {
        // Arrange
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);
        await SeedTariffAsync(TestData.NewTariffs.NewTariff2Name, TestData.NewTariffs.NewTariff2Price);

        var hourlyTariff = new Tariff
        {
            Name = TestData.ExistingTariffs.Tariff2Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff2PricePerMinute,
            BillingType = BillingType.Hourly,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
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
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff2Name, TestData.ExistingTariffs.Tariff2PricePerMinute);

        // Act
        var result = await TariffRepository.GetByBillingTypeAsync(BillingType.Hourly);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetByBillingTypeAsync_Should_ReturnOnlyActive()
    {
        // Arrange
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);

        var inactiveTariff = new Tariff
        {
            Name = TestData.ExistingTariffs.Tariff2Name,
            PricePerMinute = TestData.NewTariffs.NewTariff2Price,
            BillingType = BillingType.PerMinute,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow
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
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff3Name, TestData.ExistingTariffs.Tariff3PricePerMinute);
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff2Name, TestData.ExistingTariffs.Tariff2PricePerMinute);

        // Act
        var result = (await TariffRepository.GetByBillingTypeAsync(BillingType.PerMinute)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].PricePerMinute.Should().Be(TestData.ExistingTariffs.Tariff1PricePerMinute);
        result[1].PricePerMinute.Should().Be(TestData.ExistingTariffs.Tariff2PricePerMinute);
        result[2].PricePerMinute.Should().Be(TestData.ExistingTariffs.Tariff3PricePerMinute);
    }
}
