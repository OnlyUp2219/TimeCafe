namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class GetWithThemeByIdAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetWithThemeByIdAsync_Should_ReturnNull_WhenNotFound()
    {
        // Act
        var result = await TariffRepository.GetWithThemeByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetWithThemeByIdAsync_Should_ReturnTariffWithTheme_WhenExists()
    {
        // Arrange
        var theme = await SeedThemeAsync("Test Theme");
        var tariff = new Tariff
        {
            Name = "Test Tariff",
            PricePerMinute = 10m,
            BillingType = BillingType.PerMinute,
            ThemeId = theme.ThemeId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        Context.Tariffs.Add(tariff);
        await Context.SaveChangesAsync();

        // Act
        var result = await TariffRepository.GetWithThemeByIdAsync(tariff.TariffId);

        // Assert
        result.Should().NotBeNull();
        result!.TariffId.Should().Be(tariff.TariffId);
        result.ThemeId.Should().Be(theme.ThemeId);
        result.ThemeName.Should().Be("Test Theme");
    }

    [Fact]
    public async Task Repository_GetWithThemeByIdAsync_Should_ReturnTariffWithoutTheme_WhenThemeDoesNotExist()
    {
        // Arrange
        var tariff = new Tariff
        {
            Name = "Test Tariff",
            PricePerMinute = 10m,
            BillingType = BillingType.PerMinute,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        Context.Tariffs.Add(tariff);
        await Context.SaveChangesAsync();

        // Act
        var result = await TariffRepository.GetWithThemeByIdAsync(tariff.TariffId);

        // Assert
        result.Should().NotBeNull();
        result!.TariffId.Should().Be(tariff.TariffId);
        result.ThemeId.Should().BeNull();
        result.ThemeName.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetWithThemeByIdAsync_Should_ReturnFromCache_WhenCached()
    {
        // Arrange
        var tariff = await SeedTariffAsync();
        var oldName = tariff.Name;

        // First call fills cache
        await TariffRepository.GetWithThemeByIdAsync(tariff.TariffId);

        // Modify DB directly
        var dbTariff = await Context.Tariffs.FindAsync(tariff.TariffId);
        dbTariff!.Name = "Updated Name";
        await Context.SaveChangesAsync();

        // Act
        var result = await TariffRepository.GetWithThemeByIdAsync(tariff.TariffId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(oldName); // Should be old name from cache
        result.Name.Should().NotBe("Updated Name");
    }
}
