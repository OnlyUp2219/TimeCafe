namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class GetByIdAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnTariff_WhenExists()
    {
        // Arrange
        var seededTariff = await SeedTariffAsync("Test Tariff", 100m);

        // Act
        var result = await TariffRepository.GetByIdAsync(seededTariff.TariffId);

        // Assert
        result.Should().NotBeNull();
        result!.TariffId.Should().Be(seededTariff.TariffId);
        result.Name.Should().Be("Test Tariff");
        result.PricePerMinute.Should().Be(100m);
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await TariffRepository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        var tariff = await SeedTariffAsync("Test", 100m);
        await TariffRepository.GetByIdAsync(tariff.TariffId);
        await TariffRepository.GetByIdAsync(tariff.TariffId);

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_IncludeTheme_WhenExists()
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
        var result = await TariffRepository.GetByIdAsync(tariff.TariffId);

        // Assert
        result.Should().NotBeNull();
        result!.Theme.Should().NotBeNull();
        result.Theme!.ThemeId.Should().Be(theme.ThemeId);
    }
}
