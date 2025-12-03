namespace Venue.TimeCafe.Test.Unit.Repositories.PromotionRepository;

public class CreateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_CreateAsync_Should_ThrowException_WhenPromotionIsNull()
    {
        // Arrange
        Promotion? nullPromotion = null;

        // Act
        var act = async () => await PromotionRepository.CreateAsync(nullPromotion!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_CreatePromotion_WhenValidData()
    {
        // Arrange
        var promotion = new Promotion
        {
            Name = "New Promo",
            Description = "Test Description",
            DiscountPercent = 15m,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(30),
            IsActive = true
        };

        // Act
        var result = await PromotionRepository.CreateAsync(promotion);

        // Assert
        result.Should().NotBeNull();
        result.PromotionId.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Promo");
        result.DiscountPercent.Should().Be(15m);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_SetCreatedAt()
    {
        // Arrange
        var promotion = new Promotion
        {
            Name = "Test",
            Description = "Test",
            DiscountPercent = 10m,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };

        // Act
        var result = await PromotionRepository.CreateAsync(promotion);

        // Assert
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_PersistToDatabase()
    {
        // Arrange
        var promotion = new Promotion
        {
            Name = "Persisted",
            Description = "Test",
            DiscountPercent = 20m,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(14),
            IsActive = true
        };

        // Act
        var result = await PromotionRepository.CreateAsync(promotion);

        // Assert
        var fromDb = await Context.Promotions.FindAsync(result.PromotionId);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be("Persisted");
        fromDb.DiscountPercent.Should().Be(20m);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_InvalidateCache()
    {
        // Arrange
        var promotion = new Promotion
        {
            Name = "Cache Test",
            Description = "Test",
            DiscountPercent = 10m,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };

        // Act
        await PromotionRepository.CreateAsync(promotion);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5.5)]
    [InlineData(100)]
    public async Task Repository_CreateAsync_Should_AcceptValidDiscounts(decimal discount)
    {
        // Arrange
        var promotion = new Promotion
        {
            Name = $"Promo {discount}%",
            Description = "Test",
            DiscountPercent = discount,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };

        // Act
        var result = await PromotionRepository.CreateAsync(promotion);

        // Assert
        result.Should().NotBeNull();
        result.DiscountPercent.Should().Be(discount);
    }
}
