namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class ActivateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_ActivateAsync_Should_ReturnTrue_WhenTariffExists()
    {
        // Arrange
        var tariff = new Tariff
        {
            Name = "Inactive",
            PricePerMinute = 100m,
            BillingType = BillingType.PerMinute,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        Context.Tariffs.Add(tariff);
        await Context.SaveChangesAsync();

        // Act
        var result = await TariffRepository.ActivateAsync(tariff.TariffId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_ActivateAsync_Should_SetIsActiveToTrue()
    {
        // Arrange
        var tariff = new Tariff
        {
            Name = "To Activate",
            PricePerMinute = 100m,
            BillingType = BillingType.PerMinute,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        Context.Tariffs.Add(tariff);
        await Context.SaveChangesAsync();

        // Act
        await TariffRepository.ActivateAsync(tariff.TariffId);

        // Assert
        var fromDb = await Context.Tariffs.FindAsync(tariff.TariffId);
        fromDb.Should().NotBeNull();
        fromDb!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_ActivateAsync_Should_ReturnFalse_WhenTariffNotExists()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await TariffRepository.ActivateAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_ActivateAsync_Should_UpdateLastModified()
    {
        // Arrange
        var tariff = new Tariff
        {
            Name = "Test",
            PricePerMinute = 100m,
            BillingType = BillingType.PerMinute,
            IsActive = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            LastModified = DateTime.UtcNow.AddDays(-1)
        };
        Context.Tariffs.Add(tariff);
        await Context.SaveChangesAsync();
        var originalModified = tariff.LastModified;

        // Act
        await TariffRepository.ActivateAsync(tariff.TariffId);

        // Assert
        var fromDb = await Context.Tariffs.FindAsync(tariff.TariffId);
        fromDb!.LastModified.Should().BeAfter(originalModified);
    }

    [Fact]
    public async Task Repository_ActivateAsync_Should_InvalidateCache()
    {
        // Arrange
        var tariff = new Tariff
        {
            Name = "Cache Test",
            PricePerMinute = 100m,
            BillingType = BillingType.PerMinute,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        Context.Tariffs.Add(tariff);
        await Context.SaveChangesAsync();

        // Act
        await TariffRepository.ActivateAsync(tariff.TariffId);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
