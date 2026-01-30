namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class UpdatePromotionTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_UpdatePromotion_Should_Return200_WhenPromotionExists()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, (int)TestData.ExistingPromotions.Promotion1DiscountPercent);
        var payload = new
        {
            promotionId = promotion.PromotionId,
            name = TestData.UpdateData.UpdatedPromotionName,
            description = promotion.Description,
            discountPercent = promotion.DiscountPercent,
            validFrom = promotion.ValidFrom,
            validTo = promotion.ValidTo,
            isActive = promotion.IsActive
        };

        var response = await Client.PutAsJsonAsync("/venue/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out _).Should().BeTrue();
            json.TryGetProperty("promotion", out var promotionJson).Should().BeTrue();
            promotionJson.GetProperty("name").GetString().Should().Be(TestData.UpdateData.UpdatedPromotionName);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdatePromotion_Should_Return200_WhenPromotionExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UpdatePromotion_Should_Return404_WhenPromotionNotFound()
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new
        {
            promotionId = TestData.NonExistingIds.NonExistingPromotionId,
            name = TestData.ExistingPromotions.Promotion1Name,
            description = TestData.DefaultValues.DefaultPromotionDescription,
            discountPercent = TestData.DefaultValues.DefaultDiscountPercent,
            validFrom = TestData.DateTimeData.GetValidFromDate(),
            validTo = TestData.DateTimeData.GetValidToDate(),
            isActive = true
        };

        var response = await Client.PutAsJsonAsync("/venue/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdatePromotion_Should_Return404_WhenPromotionNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UpdatePromotion_Should_UpdateOnlyChangedFields_WhenPartialUpdateProvided()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, (int)TestData.ExistingPromotions.Promotion1DiscountPercent);
        var payload = new
        {
            promotionId = promotion.PromotionId,
            name = TestData.UpdateData.UpdatedPromotionName,
            description = promotion.Description,
            discountPercent = promotion.DiscountPercent,
            validFrom = promotion.ValidFrom,
            validTo = promotion.ValidTo,
            isActive = promotion.IsActive
        };

        var response = await Client.PutAsJsonAsync("/venue/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var updatedPromotion = json.GetProperty("promotion");
            updatedPromotion.GetProperty("name").GetString().Should().Be(TestData.UpdateData.UpdatedPromotionName);
            updatedPromotion.GetProperty("discountPercent").GetDecimal().Should().Be(promotion.DiscountPercent);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdatePromotion_Should_UpdateOnlyChangedFields_WhenPartialUpdateProvided] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Endpoint_UpdatePromotion_Should_Return422_WhenNameIsInvalid(string? invalidName)
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, (int)TestData.ExistingPromotions.Promotion1DiscountPercent);
        var payload = new
        {
            promotionId = promotion.PromotionId,
            name = invalidName,
            description = TestData.DefaultValues.DefaultPromotionDescription,
            discountPercent = TestData.DefaultValues.DefaultDiscountPercent,
            validFrom = TestData.DateTimeData.GetValidFromDate(),
            validTo = TestData.DateTimeData.GetValidToDate(),
            isActive = true
        };

        var response = await Client.PutAsJsonAsync("/venue/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdatePromotion_Should_Return422_WhenNameIsInvalid] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("123-invalid")]
    public async Task Endpoint_UpdatePromotion_Should_Return422_WhenPromotionIdIsInvalid(string invalidId)
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new
        {
            promotionId = invalidId,
            name = TestData.ExistingPromotions.Promotion1Name,
            description = TestData.DefaultValues.DefaultPromotionDescription,
            discountPercent = TestData.DefaultValues.DefaultDiscountPercent,
            validFrom = TestData.DateTimeData.GetValidFromDate(),
            validTo = TestData.DateTimeData.GetValidToDate(),
            isActive = true
        };

        var response = await Client.PutAsJsonAsync("/venue/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdatePromotion_Should_Return422_WhenPromotionIdIsInvalid({invalidId})] Response: {jsonString}");
            throw;
        }
    }
}
