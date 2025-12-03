namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class GetAllAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnAllTariffs_WhenExist()
    {
        // Arrange
        await SeedTariffAsync("Tariff 1", 100m);
        await SeedTariffAsync("Tariff 2", 200m);
        await SeedTariffAsync("Tariff 3", 300m);

        // Act
        var result = await TariffRepository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnEmptyList_WhenNoTariffs()
    {
        // Act
        var result = await TariffRepository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_OrderByCreatedAtDescending()
    {
        // Arrange
        var tariff1 = await SeedTariffAsync("First", 100m);
        await Task.Delay(10);
        var tariff2 = await SeedTariffAsync("Second", 200m);
        await Task.Delay(10);
        var tariff3 = await SeedTariffAsync("Third", 300m);

        // Act
        var result = (await TariffRepository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Third");
        result[1].Name.Should().Be("Second");
        result[2].Name.Should().Be("First");
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnFromCache_WhenCached()
    {
        // Arrange
        await SeedTariffAsync("Tariff 1", 100m);

        // First call - should cache
        var firstResult = await TariffRepository.GetAllAsync();

        // Act - Second call should return from cache
        var secondResult = await TariffRepository.GetAllAsync();

        // Assert
        secondResult.Should().NotBeNull();
        secondResult.Should().HaveCount(1);
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_IncludeThemes_WhenExist()
    {
        // Arrange
        var theme = await SeedThemeAsync("Test Theme");
        var tariff = new Tariff
        {
            Name = "Tariff with Theme",
            PricePerMinute = 200m,
            BillingType = BillingType.PerMinute,
            ThemeId = theme.ThemeId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        Context.Tariffs.Add(tariff);
        await Context.SaveChangesAsync();

        // Act
        var result = await TariffRepository.GetAllAsync();

        // Assert
        var tariffWithTheme = result.First();
        tariffWithTheme.Theme.Should().NotBeNull();
        tariffWithTheme.Theme!.ThemeId.Should().Be(theme.ThemeId);
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnBothActiveAndInactive()
    {
        // Arrange
        var activeTariff = await SeedTariffAsync("Active", 100m);
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
        var result = await TariffRepository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.IsActive);
        result.Should().Contain(t => !t.IsActive);
    }
}
