namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class GetActiveAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetActiveAsync_Should_ReturnOnlyActiveTariffs()
    {
        // Arrange
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff2Name, TestData.ExistingTariffs.Tariff2PricePerMinute);

        var inactiveTariff = new Tariff
        {
            Name = TestData.ExistingTariffs.Tariff3Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff3PricePerMinute,
            BillingType = BillingType.PerMinute,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        Context.Tariffs.Add(inactiveTariff);
        await Context.SaveChangesAsync();

        // Act
        var result = await TariffRepository.GetActiveAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.TariffIsActive);
    }

    [Fact]
    public async Task Repository_GetActiveAsync_Should_ReturnEmptyList_WhenNoActiveTariffs()
    {
        // Arrange
        var inactiveTariff = new Tariff
        {
            Name = TestData.DefaultValues.DefaultTariffName,
            PricePerMinute = TestData.DefaultValues.DefaultTariffPrice,
            BillingType = BillingType.PerMinute,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        Context.Tariffs.Add(inactiveTariff);
        await Context.SaveChangesAsync();

        // Act
        var result = await TariffRepository.GetActiveAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetActiveAsync_Should_OrderByName()
    {
        // Arrange
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff3Name, TestData.ExistingTariffs.Tariff3PricePerMinute);
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff2Name, TestData.ExistingTariffs.Tariff2PricePerMinute);

        // Act
        var result = (await TariffRepository.GetActiveAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].TariffName.Should().Be(TestData.ExistingTariffs.Tariff1Name);
        result[1].TariffName.Should().Be(TestData.ExistingTariffs.Tariff2Name);
        result[2].TariffName.Should().Be(TestData.ExistingTariffs.Tariff3Name);
    }

    [Fact]
    public async Task Repository_GetActiveAsync_Should_ReturnFromCache_WhenCached()
    {
        // Arrange
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);

        // First call - should cache
        var firstResult = await TariffRepository.GetActiveAsync();

        // Act - Second call should return from cache
        var secondResult = await TariffRepository.GetActiveAsync();

        // Assert
        secondResult.Should().NotBeNull();
        secondResult.Should().HaveCount(1);
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }
}
