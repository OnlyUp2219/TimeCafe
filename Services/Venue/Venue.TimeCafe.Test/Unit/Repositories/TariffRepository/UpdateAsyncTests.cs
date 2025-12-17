namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class UpdateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_UpdateAsync_Should_ThrowException_WhenTariffIsNull()
    {
        // Arrange
        Tariff? nullTariff = null;

        // Act
        var act = async () => await TariffRepository.UpdateAsync(nullTariff!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateTariff_WhenExists()
    {
        // Arrange
        var existing = await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);
        existing.Name = TestData.ExistingTariffs.Tariff2Name;
        existing.PricePerMinute = TestData.ExistingTariffs.Tariff2PricePerMinute;

        // Act
        var result = await TariffRepository.UpdateAsync(existing);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(TestData.ExistingTariffs.Tariff2Name);
        result.PricePerMinute.Should().Be(TestData.ExistingTariffs.Tariff2PricePerMinute);
        result.LastModified.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistent = new Tariff
        {
            TariffId = TestData.NonExistingIds.NonExistingTariffId,
            Name = TestData.DefaultValues.DefaultTariffName,
            PricePerMinute = TestData.DefaultValues.DefaultTariffPrice,
            BillingType = TestData.DefaultValues.DefaultBillingType
        };

        // Act
        var result = await TariffRepository.UpdateAsync(nonExistent);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateLastModified()
    {
        // Arrange
        var existing = await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);
        var originalModified = existing.LastModified;
        await Task.Delay(100);

        existing.Name = TestData.ExistingTariffs.Tariff1Name;

        // Act
        var result = await TariffRepository.UpdateAsync(existing);

        // Assert
        result.LastModified.Should().BeAfter(originalModified);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_InvalidateCache()
    {
        // Arrange
        var existing = await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);
        existing.Name = TestData.ExistingTariffs.Tariff2Name;

        // Act
        await TariffRepository.UpdateAsync(existing);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_PersistChanges()
    {
        // Arrange
        var existing = await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);
        existing.Name = TestData.ExistingTariffs.Tariff3Name;
        existing.PricePerMinute = TestData.ExistingTariffs.Tariff3PricePerMinute;

        // Act
        await TariffRepository.UpdateAsync(existing);

        // Assert
        var fromDb = await Context.Tariffs.FindAsync(existing.TariffId);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be(TestData.ExistingTariffs.Tariff3Name);
        fromDb.PricePerMinute.Should().Be(TestData.ExistingTariffs.Tariff3PricePerMinute);
    }
}
