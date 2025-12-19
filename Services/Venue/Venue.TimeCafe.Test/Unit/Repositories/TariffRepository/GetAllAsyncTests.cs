namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class GetAllAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnAllTariffs_WhenExist()
    {
        // Arrange
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff2Name, TestData.ExistingTariffs.Tariff2PricePerMinute);
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff3Name, TestData.ExistingTariffs.Tariff3PricePerMinute);

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
        var tariff1 = await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);
        await Task.Delay(10);
        var tariff2 = await SeedTariffAsync(TestData.ExistingTariffs.Tariff2Name, TestData.ExistingTariffs.Tariff2PricePerMinute);
        await Task.Delay(10);
        var tariff3 = await SeedTariffAsync(TestData.ExistingTariffs.Tariff3Name, TestData.ExistingTariffs.Tariff3PricePerMinute);

        // Act
        var result = (await TariffRepository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be(TestData.ExistingTariffs.Tariff3Name);
        result[1].Name.Should().Be(TestData.ExistingTariffs.Tariff2Name);
        result[2].Name.Should().Be(TestData.ExistingTariffs.Tariff1Name);
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnFromCache_WhenCached()
    {
        // Arrange
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);

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
        var theme = await SeedThemeAsync(TestData.ExistingThemes.Theme1Name);
        var tariff = new Tariff
        {
            Name = TestData.ExistingTariffs.Tariff2Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff2PricePerMinute,
            BillingType = BillingType.PerMinute,
            ThemeId = theme.ThemeId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        Context.Tariffs.Add(tariff);
        await Context.SaveChangesAsync();

        // Act
        var result = await TariffRepository.GetAllAsync();

        // Assert
        var tariffWithTheme = result.First();
        tariffWithTheme.ThemeId.Should().Be(theme.ThemeId);
    }

    [Fact]
    public async Task Repository_GetAllAsync_Should_ReturnBothActiveAndInactive()
    {
        // Arrange
        var activeTariff = await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);
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
        var result = await TariffRepository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.IsActive);
        result.Should().Contain(t => !t.IsActive);
    }
}
