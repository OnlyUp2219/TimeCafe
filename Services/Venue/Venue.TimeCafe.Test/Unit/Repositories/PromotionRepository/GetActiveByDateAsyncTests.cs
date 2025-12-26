namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class GetActiveByDateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetActiveByDateAsync_Should_ReturnActivePromotionsForDate()
    {
        // Arrange
        var targetDate = DateTimeOffset.UtcNow.AddDays(5);
        var promotion = new Promotion
        {
            Name = TestData.ExistingPromotions.Promotion1Name,
            Description = TestData.ExistingPromotions.Promotion1Description,
            DiscountPercent = TestData.ExistingPromotions.Promotion1DiscountPercent,
            ValidFrom = TestData.DateTimeData.GetValidFromDate(),
            ValidTo = TestData.DateTimeData.GetValidToDate(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        Context.Promotions.Add(promotion);
        await Context.SaveChangesAsync();

        // Act
        var result = await PromotionRepository.GetActiveByDateAsync(targetDate);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be(TestData.ExistingPromotions.Promotion1Name);
    }

    [Fact]
    public async Task Repository_GetActiveByDateAsync_Should_ExcludeExpiredPromotions()
    {
        // Arrange
        var targetDate = DateTimeOffset.UtcNow;
        var expired = new Promotion
        {
            Name = TestData.ExistingPromotions.Promotion2Name,
            Description = TestData.ExistingPromotions.Promotion2Description,
            DiscountPercent = TestData.ExistingPromotions.Promotion2DiscountPercent,
            ValidFrom = TestData.DateTimeData.GetPastDate(),
            ValidTo = DateTimeOffset.UtcNow.AddDays(-1),
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
        var targetDate = DateTimeOffset.UtcNow;
        var future = new Promotion
        {
            Name = TestData.ExistingPromotions.Promotion3Name,
            Description = TestData.ExistingPromotions.Promotion3Description,
            DiscountPercent = TestData.ExistingPromotions.Promotion3DiscountPercent,
            ValidFrom = TestData.DateTimeData.GetFutureDate(),
            ValidTo = TestData.DateTimeData.GetFutureDate().AddDays(30),
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
        var targetDate = DateTimeOffset.UtcNow.AddDays(5);
        var inactive = new Promotion
        {
            Name = "Inactive",
            Description = "Test",
            DiscountPercent = 10m,
            ValidFrom = DateTimeOffset.UtcNow,
            ValidTo = DateTimeOffset.UtcNow.AddDays(10),
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
        var targetDate = DateTimeOffset.UtcNow.AddDays(5);
        var promo1 = new Promotion
        {
            Name = "Low Discount",
            Description = "Test",
            DiscountPercent = 10m,
            ValidFrom = DateTimeOffset.UtcNow,
            ValidTo = DateTimeOffset.UtcNow.AddDays(10),
            IsActive = true
        };
        var promo2 = new Promotion
        {
            Name = "High Discount",
            Description = "Test",
            DiscountPercent = 50m,
            ValidFrom = DateTimeOffset.UtcNow,
            ValidTo = DateTimeOffset.UtcNow.AddDays(10),
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
        var validFrom = DateTimeOffset.UtcNow.Date;
        var validTo = DateTimeOffset.UtcNow.Date.AddDays(10);
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
