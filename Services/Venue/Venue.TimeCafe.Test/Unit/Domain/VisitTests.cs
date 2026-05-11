using FluentAssertions;
using Venue.TimeCafe.Domain.Models;
using Xunit;

namespace Venue.TimeCafe.Test.Unit.Domain;

public class VisitTests
{
    [Fact]
    public void CalculateCost_Should_ApplyOnlyPersonalDiscount()
    {
        // Arrange
        var entryTime = DateTimeOffset.UtcNow;
        var exitTime = entryTime.AddMinutes(100); // 100 minutes
        var pricePerMinute = 1m; // 100 total base cost
        
        // Act
        var cost = Visit.CalculateCost(
            tariffBillingType: BillingType.PerMinute,
            tariffPricePerMinute: pricePerMinute,
            exitTime: exitTime,
            entryTime: entryTime,
            personalDiscount: 10m // 10%
        );

        // Assert
        // Base cost = 100 * 1 = 100
        // Discount = 10%
        // Expected = 90
        cost.Should().Be(90m);
    }

    [Fact]
    public void CalculateCost_Should_ApplyPersonalAndTariffDiscount_WhenTariffIsBetterThanGlobal()
    {
        // Arrange
        var entryTime = DateTimeOffset.UtcNow;
        var exitTime = entryTime.AddMinutes(100);
        var pricePerMinute = 1m;
        
        // Act
        var cost = Visit.CalculateCost(
            tariffBillingType: BillingType.PerMinute,
            tariffPricePerMinute: pricePerMinute,
            exitTime: exitTime,
            entryTime: entryTime,
            personalDiscount: 10m, // 10%
            tariffDiscount: 20m,   // 20% (Better)
            globalDiscount: 15m    // 15%
        );

        // Assert
        // Base cost = 100
        // Best promo = 20% (Tariff)
        // Total discount = 20 + 10 = 30%
        // Expected = 70
        cost.Should().Be(70m);
    }

    [Fact]
    public void CalculateCost_Should_ApplyPersonalAndGlobalDiscount_WhenGlobalIsBetterThanTariff()
    {
        // Arrange
        var entryTime = DateTimeOffset.UtcNow;
        var exitTime = entryTime.AddMinutes(100);
        var pricePerMinute = 1m;
        
        // Act
        var cost = Visit.CalculateCost(
            tariffBillingType: BillingType.PerMinute,
            tariffPricePerMinute: pricePerMinute,
            exitTime: exitTime,
            entryTime: entryTime,
            personalDiscount: 10m, // 10%
            tariffDiscount: 15m,   // 15%
            globalDiscount: 25m    // 25% (Better)
        );

        // Assert
        // Base cost = 100
        // Best promo = 25% (Global)
        // Total discount = 25 + 10 = 35%
        // Expected = 65
        cost.Should().Be(65m);
    }

    [Fact]
    public void CalculateCost_Should_CapDiscount_AtMaxDiscountPercent()
    {
        // Arrange
        var entryTime = DateTimeOffset.UtcNow;
        var exitTime = entryTime.AddMinutes(100);
        var pricePerMinute = 1m;
        
        // Act
        var cost = Visit.CalculateCost(
            tariffBillingType: BillingType.PerMinute,
            tariffPricePerMinute: pricePerMinute,
            exitTime: exitTime,
            entryTime: entryTime,
            personalDiscount: 30m,
            tariffDiscount: 30m,
            maxDiscountPercent: 50m // Cap at 50%
        );

        // Assert
        // Base cost = 100
        // Sum = 60%
        // Capped = 50%
        // Expected = 50
        cost.Should().Be(50m);
    }

    [Fact]
    public void CalculateCost_Should_CalculateHourlyCorrectly()
    {
        // Arrange
        var entryTime = DateTimeOffset.UtcNow;
        var exitTime = entryTime.AddMinutes(65); // 1 hour and 5 minutes -> should be 2 hours
        var pricePerMinute = 1m; // 60 per hour
        
        // Act
        var cost = Visit.CalculateCost(
            tariffBillingType: BillingType.Hourly,
            tariffPricePerMinute: pricePerMinute,
            exitTime: exitTime,
            entryTime: entryTime
        );

        // Assert
        // Hours = ceil(65/60) = 2
        // Base cost = 2 * 1 * 60 = 120
        // Discount = 0
        // Expected = 120
        cost.Should().Be(120m);
    }

    [Fact]
    public void CalculateCost_Should_CalculatePerMinuteCorrectly()
    {
        // Arrange
        var entryTime = DateTimeOffset.UtcNow;
        var exitTime = entryTime.AddMinutes(65.5); // 65.5 minutes -> should be 66 minutes
        var pricePerMinute = 1m;
        
        // Act
        var cost = Visit.CalculateCost(
            tariffBillingType: BillingType.PerMinute,
            tariffPricePerMinute: pricePerMinute,
            exitTime: exitTime,
            entryTime: entryTime
        );

        // Assert
        // Minutes = ceil(65.5) = 66
        // Base cost = 66 * 1 = 66
        // Discount = 0
        // Expected = 66
        cost.Should().Be(66m);
    }
}
