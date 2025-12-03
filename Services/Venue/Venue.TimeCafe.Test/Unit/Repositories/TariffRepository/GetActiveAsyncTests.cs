namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class GetActiveAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetActiveAsync_Should_ReturnOnlyActiveTariffs()
    {
        // Arrange
        await SeedTariffAsync("Active 1", 100m);
        await SeedTariffAsync("Active 2", 200m);

        var inactiveTariff = new Tariff
        {
            Name = "Inactive",
            PricePerMinute = 300m,
            BillingType = BillingType.PerMinute,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        Context.Tariffs.Add(inactiveTariff);
        await Context.SaveChangesAsync();

        // Act
        var result = await TariffRepository.GetActiveAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.IsActive);
    }

    [Fact]
    public async Task Repository_GetActiveAsync_Should_ReturnEmptyList_WhenNoActiveTariffs()
    {
        // Arrange
        var inactiveTariff = new Tariff
        {
            Name = "Inactive",
            PricePerMinute = 100m,
            BillingType = BillingType.PerMinute,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
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
        await SeedTariffAsync("Charlie", 100m);
        await SeedTariffAsync("Alpha", 200m);
        await SeedTariffAsync("Bravo", 300m);

        // Act
        var result = (await TariffRepository.GetActiveAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alpha");
        result[1].Name.Should().Be("Bravo");
        result[2].Name.Should().Be("Charlie");
    }

    [Fact]
    public async Task Repository_GetActiveAsync_Should_ReturnFromCache_WhenCached()
    {
        // Arrange
        await SeedTariffAsync("Active", 100m);

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
