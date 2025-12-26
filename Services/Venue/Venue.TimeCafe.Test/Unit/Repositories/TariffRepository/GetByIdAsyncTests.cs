namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class GetByIdAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnTariff_WhenExists()
    {
        // Arrange
        var seededTariff = await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);

        // Act
        var result = await TariffRepository.GetByIdAsync(seededTariff.TariffId);

        // Assert
        result.Should().NotBeNull();
        result!.TariffId.Should().Be(seededTariff.TariffId);
        result.Name.Should().Be(TestData.DefaultValues.DefaultTariffName);
        result.PricePerMinute.Should().Be(TestData.DefaultValues.DefaultTariffPrice);
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = TestData.NonExistingIds.NonExistingTariffId;

        // Act
        var result = await TariffRepository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_RequestCache_OnMultipleCalls()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);
        await TariffRepository.GetByIdAsync(tariff.TariffId);
        await TariffRepository.GetByIdAsync(tariff.TariffId);

        // Assert
        CacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_IncludeTheme_WhenExists()
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
        var result = await TariffRepository.GetByIdAsync(tariff.TariffId);

        // Assert
        result.Should().NotBeNull();
        result!.ThemeId.Should().Be(theme.ThemeId);
    }
}
