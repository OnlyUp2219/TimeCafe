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
        var existing = await SeedTariffAsync("Original", 100m);
        existing.Name = "Updated";
        existing.PricePerMinute = 200m;

        // Act
        var result = await TariffRepository.UpdateAsync(existing);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated");
        result.PricePerMinute.Should().Be(200m);
        result.LastModified.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistent = new Tariff
        {
            TariffId = 99999,
            Name = "Non-existent",
            PricePerMinute = 100m,
            BillingType = BillingType.PerMinute
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
        var existing = await SeedTariffAsync("Test", 100m);
        var originalModified = existing.LastModified;
        await Task.Delay(100);

        existing.Name = "Modified";

        // Act
        var result = await TariffRepository.UpdateAsync(existing);

        // Assert
        result.LastModified.Should().BeAfter(originalModified);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_InvalidateCache()
    {
        // Arrange
        var existing = await SeedTariffAsync("Original", 100m);
        existing.Name = "Updated";

        // Act
        await TariffRepository.UpdateAsync(existing);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_PersistChanges()
    {
        // Arrange
        var existing = await SeedTariffAsync("Original", 100m);
        existing.Name = "Persisted Update";
        existing.PricePerMinute = 250m;

        // Act
        await TariffRepository.UpdateAsync(existing);

        // Assert
        var fromDb = await Context.Tariffs.FindAsync(existing.TariffId);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be("Persisted Update");
        fromDb.PricePerMinute.Should().Be(250m);
    }
}
