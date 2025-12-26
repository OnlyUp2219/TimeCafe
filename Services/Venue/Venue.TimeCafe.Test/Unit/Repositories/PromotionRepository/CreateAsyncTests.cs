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
            Name = TestData.NewPromotions.NewPromotion1Name,
            Description = TestData.NewPromotions.NewPromotion1Description,
            DiscountPercent = TestData.NewPromotions.NewPromotion1DiscountPercent,
            ValidFrom = TestData.DateTimeData.GetValidFromDate(),
            ValidTo = TestData.DateTimeData.GetValidToDate(),
            IsActive = true
        };

        // Act
        var result = await PromotionRepository.CreateAsync(promotion);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(TestData.NewPromotions.NewPromotion1Name);
        result.DiscountPercent.Should().Be(TestData.NewPromotions.NewPromotion1DiscountPercent);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_SetCreatedAt()
    {
        // Arrange
        var promotion = new Promotion
        {
            Name = TestData.DefaultValues.DefaultPromotionName,
            Description = TestData.DefaultValues.DefaultPromotionDescription,
            DiscountPercent = TestData.DefaultValues.DefaultDiscountPercent,
            ValidFrom = TestData.DateTimeData.GetValidFromDate(),
            ValidTo = TestData.DateTimeData.GetValidToDate(),
            IsActive = true
        };

        // Act
        var result = await PromotionRepository.CreateAsync(promotion);

        // Assert
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_PersistToDatabase()
    {
        // Arrange
        var promotion = new Promotion
        {
            Name = TestData.NewPromotions.NewPromotion2Name,
            Description = TestData.NewPromotions.NewPromotion2Description,
            DiscountPercent = TestData.NewPromotions.NewPromotion2DiscountPercent,
            ValidFrom = TestData.DateTimeData.GetValidFromDate(),
            ValidTo = TestData.DateTimeData.GetValidToDate(),
            IsActive = true
        };

        // Act
        var result = await PromotionRepository.CreateAsync(promotion);

        // Assert
        var fromDb = await Context.Promotions.FindAsync(result.PromotionId);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be(TestData.NewPromotions.NewPromotion2Name);
        fromDb.DiscountPercent.Should().Be(TestData.NewPromotions.NewPromotion2DiscountPercent);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_InvalidateCache()
    {
        // Arrange
        var promotion = new Promotion
        {
            Name = TestData.DefaultValues.DefaultPromotionName,
            Description = TestData.DefaultValues.DefaultPromotionDescription,
            DiscountPercent = TestData.DefaultValues.DefaultDiscountPercent,
            ValidFrom = TestData.DateTimeData.GetValidFromDate(),
            ValidTo = TestData.DateTimeData.GetValidToDate(),
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
            Name = $"{TestData.DefaultValues.DefaultPromotionName} {discount}%",
            Description = TestData.DefaultValues.DefaultPromotionDescription,
            DiscountPercent = discount,
            ValidFrom = TestData.DateTimeData.GetValidFromDate(),
            ValidTo = TestData.DateTimeData.GetValidToDate(),
            IsActive = true
        };

        // Act
        var result = await PromotionRepository.CreateAsync(promotion);

        // Assert
        result.Should().NotBeNull();
        result.DiscountPercent.Should().Be(discount);
    }
}
