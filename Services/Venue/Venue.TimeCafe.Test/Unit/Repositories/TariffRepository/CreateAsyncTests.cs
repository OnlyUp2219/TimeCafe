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
            Name = TestData.NewTariffs.NewTariff1Name,
            PricePerMinute = TestData.NewTariffs.NewTariff1Price,
            BillingType = TestData.NewTariffs.NewTariff1BillingType,
            IsActive = true
        };

        // Act
        var result = await TariffRepository.CreateAsync(tariff);

        // Assert
        result.Should().NotBeNull();
        result.TariffId.Should().NotBe(Guid.Empty);
        result.Name.Should().Be(TestData.NewTariffs.NewTariff1Name);
        result.PricePerMinute.Should().Be(TestData.NewTariffs.NewTariff1Price);
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        result.LastModified.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_SetTimestamps()
    {
        // Arrange
        var tariff = new Tariff
        {
            Name = TestData.DefaultValues.DefaultTariffName,
            PricePerMinute = TestData.DefaultValues.DefaultTariffPrice,
            BillingType = TestData.DefaultValues.DefaultBillingType,
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
            Name = TestData.ExistingTariffs.Tariff2Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff2PricePerMinute,
            BillingType = TestData.ExistingTariffs.Tariff2BillingType,
            IsActive = true
        };

        // Act
        var result = await TariffRepository.CreateAsync(tariff);

        // Assert
        var fromDb = await Context.Tariffs.FindAsync(result.TariffId);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be(TestData.ExistingTariffs.Tariff2Name);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_InvalidateCache()
    {
        // Arrange
        var tariff = new Tariff
        {
            Name = TestData.DefaultValues.DefaultTariffName,
            PricePerMinute = TestData.DefaultValues.DefaultTariffPrice,
            BillingType = TestData.DefaultValues.DefaultBillingType,
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
        var theme = await SeedThemeAsync(TestData.ExistingThemes.Theme1Name);
        var tariff = new Tariff
        {
            Name = TestData.ExistingTariffs.Tariff1Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            BillingType = TestData.ExistingTariffs.Tariff1BillingType,
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
