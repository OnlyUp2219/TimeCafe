namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class ActivateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_ActivateAsync_Should_ReturnTrue_WhenTariffExists()
    {
        // Arrange
        var tariff = new Tariff
        {
            Name = TestData.ExistingTariffs.Tariff1Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            BillingType = TestData.ExistingTariffs.Tariff1BillingType,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow
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
            Name = TestData.ExistingTariffs.Tariff2Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff2PricePerMinute,
            BillingType = TestData.ExistingTariffs.Tariff2BillingType,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow
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
        var nonExistentId = TestData.NonExistingIds.NonExistingTariffId;

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
            Name = TestData.DefaultValues.DefaultTariffName,
            PricePerMinute = TestData.DefaultValues.DefaultTariffPrice,
            BillingType = TestData.DefaultValues.DefaultBillingType,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            LastModified = DateTimeOffset.UtcNow.AddDays(-1)
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
            Name = TestData.ExistingTariffs.Tariff3Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff3PricePerMinute,
            BillingType = TestData.ExistingTariffs.Tariff3BillingType,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        Context.Tariffs.Add(tariff);
        await Context.SaveChangesAsync();

        // Act
        await TariffRepository.ActivateAsync(tariff.TariffId);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
