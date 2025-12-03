namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class CreateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_CreateAsync_Should_ThrowException_WhenTariffIsNull()
    {
        // Arrange
        Tariff? nullTariff = null;

        // Act
        var act = async () => await TariffRepository.CreateAsync(nullTariff!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_CreateTariff_WhenValidData()
    {
        // Arrange
        var tariff = new Tariff
        {
            Name = "New Tariff",
            PricePerMinute = 150m,
            BillingType = BillingType.PerMinute,
            IsActive = true
        };

        // Act
        var result = await TariffRepository.CreateAsync(tariff);

        // Assert
        result.Should().NotBeNull();
        result.TariffId.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Tariff");
        result.PricePerMinute.Should().Be(150m);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.LastModified.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_SetTimestamps()
    {
        // Arrange
        var tariff = new Tariff
        {
            Name = "Test",
            PricePerMinute = 100m,
            BillingType = BillingType.PerMinute,
            IsActive = true
        };

        // Act
        var result = await TariffRepository.CreateAsync(tariff);

        // Assert
        result.CreatedAt.Should().NotBe(default);
        result.LastModified.Should().NotBe(default);
        result.CreatedAt.Should().BeCloseTo(result.LastModified, TimeSpan.FromMilliseconds(10));
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_PersistToDatabase()
    {
        // Arrange
        var tariff = new Tariff
        {
            Name = "Persisted",
            PricePerMinute = 200m,
            BillingType = BillingType.PerMinute,
            IsActive = true
        };

        // Act
        var result = await TariffRepository.CreateAsync(tariff);

        // Assert
        var fromDb = await Context.Tariffs.FindAsync(result.TariffId);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be("Persisted");
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_InvalidateCache()
    {
        // Arrange
        var tariff = new Tariff
        {
            Name = "Cache Test",
            PricePerMinute = 100m,
            BillingType = BillingType.PerMinute,
            IsActive = true
        };

        // Act
        await TariffRepository.CreateAsync(tariff);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(9999.99)]
    public async Task Repository_CreateAsync_Should_AcceptValidPrices(decimal price)
    {
        // Arrange
        var tariff = new Tariff
        {
            Name = $"Tariff {price}",
            PricePerMinute = price,
            BillingType = BillingType.PerMinute,
            IsActive = true
        };

        // Act
        var result = await TariffRepository.CreateAsync(tariff);

        // Assert
        result.Should().NotBeNull();
        result.PricePerMinute.Should().Be(price);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_CreateWithTheme_WhenThemeIdProvided()
    {
        // Arrange
        var theme = await SeedThemeAsync("Test Theme");
        var tariff = new Tariff
        {
            Name = "Tariff with Theme",
            PricePerMinute = 100m,
            BillingType = BillingType.PerMinute,
            ThemeId = theme.ThemeId,
            IsActive = true
        };

        // Act
        var result = await TariffRepository.CreateAsync(tariff);

        // Assert
        result.Should().NotBeNull();
        result.ThemeId.Should().Be(theme.ThemeId);
    }
}
