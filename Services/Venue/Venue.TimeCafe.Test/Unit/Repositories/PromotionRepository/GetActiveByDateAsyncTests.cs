namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class GetActiveByDateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetActiveByDateAsync_Should_ReturnActivePromotionsForDate()
    {
        // Arrange
        var targetDate = DateTime.UtcNow.AddDays(5);
        var promotion = new Promotion
        {
            Name = "Valid Promo",
            Description = "Test",
            DiscountPercent = 15m,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(10),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        Context.Promotions.Add(promotion);
        await Context.SaveChangesAsync();

        // Act
        var result = await PromotionRepository.GetActiveByDateAsync(targetDate);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Valid Promo");
    }

    [Fact]
    public async Task Repository_GetActiveByDateAsync_Should_ExcludeExpiredPromotions()
    {
        // Arrange
        var targetDate = DateTime.UtcNow;
        var expired = new Promotion
        {
            Name = "Expired",
            Description = "Test",
            DiscountPercent = 10m,
            ValidFrom = DateTime.UtcNow.AddDays(-10),
            ValidTo = DateTime.UtcNow.AddDays(-1),
            IsActive = true
        };
        Context.Promotions.Add(expired);
        await Context.SaveChangesAsync();

        // Act
        var result = await PromotionRepository.GetActiveByDateAsync(targetDate);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetActiveByDateAsync_Should_ExcludeFuturePromotions()
    {
        // Arrange
        var targetDate = DateTime.UtcNow;
        var future = new Promotion
        {
            Name = "Future",
            Description = "Test",
            DiscountPercent = 10m,
            ValidFrom = DateTime.UtcNow.AddDays(5),
            ValidTo = DateTime.UtcNow.AddDays(10),
            IsActive = true
        };
        Context.Promotions.Add(future);
        await Context.SaveChangesAsync();

        // Act
        var result = await PromotionRepository.GetActiveByDateAsync(targetDate);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetActiveByDateAsync_Should_ExcludeInactivePromotions()
    {
        // Arrange
        var targetDate = DateTime.UtcNow.AddDays(5);
        var inactive = new Promotion
        {
            Name = "Inactive",
            Description = "Test",
            DiscountPercent = 10m,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(10),
            IsActive = false
        };
        Context.Promotions.Add(inactive);
        await Context.SaveChangesAsync();

        // Act
        var result = await PromotionRepository.GetActiveByDateAsync(targetDate);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetActiveByDateAsync_Should_ReturnOrderedByDiscountDesc()
    {
        // Arrange
        var targetDate = DateTime.UtcNow.AddDays(5);
        var promo1 = new Promotion
        {
            Name = "Low Discount",
            Description = "Test",
            DiscountPercent = 10m,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(10),
            IsActive = true
        };
        var promo2 = new Promotion
        {
            Name = "High Discount",
            Description = "Test",
            DiscountPercent = 50m,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(10),
            IsActive = true
        };
        Context.Promotions.AddRange(promo1, promo2);
        await Context.SaveChangesAsync();

        // Act
        var result = (await PromotionRepository.GetActiveByDateAsync(targetDate)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("High Discount");
        result[1].Name.Should().Be("Low Discount");
    }

    [Fact]
    public async Task Repository_GetActiveByDateAsync_Should_IncludeBoundaryDates()
    {
        // Arrange
        var validFrom = DateTime.UtcNow.Date;
        var validTo = DateTime.UtcNow.Date.AddDays(10);
        var promotion = new Promotion
        {
            Name = "Boundary Test",
            Description = "Test",
            DiscountPercent = 20m,
            ValidFrom = validFrom,
            ValidTo = validTo,
            IsActive = true
        };
        Context.Promotions.Add(promotion);
        await Context.SaveChangesAsync();

        // Act - Test on ValidFrom
        var resultStart = await PromotionRepository.GetActiveByDateAsync(validFrom);
        // Act - Test on ValidTo
        var resultEnd = await PromotionRepository.GetActiveByDateAsync(validTo);

        // Assert
        resultStart.Should().HaveCount(1);
        resultEnd.Should().HaveCount(1);
    }
}
