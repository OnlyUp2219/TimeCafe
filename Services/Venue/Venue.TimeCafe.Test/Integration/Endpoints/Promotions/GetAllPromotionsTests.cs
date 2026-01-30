namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class GetAllPromotionsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetAllPromotions_Should_Return200_WhenPromotionsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, (int)TestData.ExistingPromotions.Promotion1DiscountPercent);
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, (int)TestData.ExistingPromotions.Promotion2DiscountPercent);
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion3Name, (int)TestData.ExistingPromotions.Promotion3DiscountPercent);

        var response = await Client.GetAsync("/venue/promotions");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("promotions", out var promotions).Should().BeTrue();
            promotions.ValueKind.Should().Be(JsonValueKind.Array);
            promotions.GetArrayLength().Should().BeGreaterThanOrEqualTo(3);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAllPromotions_Should_Return200_WhenPromotionsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetAllPromotions_Should_Return200_WhenNoPromotionsExist()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.GetAsync("/venue/promotions");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("promotions", out var promotions).Should().BeTrue();
            promotions.ValueKind.Should().Be(JsonValueKind.Array);
            promotions.GetArrayLength().Should().Be(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAllPromotions_Should_Return200_WhenNoPromotionsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetAllPromotions_Should_ReturnAllProperties_WhenPromotionsExist()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync(TestData.NewPromotions.NewPromotion1Name, (int)TestData.NewPromotions.NewPromotion1DiscountPercent);

        var response = await Client.GetAsync("/venue/promotions");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var promotions = json.GetProperty("promotions");
            var foundPromotion = promotions.EnumerateArray().FirstOrDefault(p => p.GetProperty("name").GetString() == TestData.NewPromotions.NewPromotion1Name);

            foundPromotion.ValueKind.Should().NotBe(JsonValueKind.Undefined);
            foundPromotion.GetProperty("promotionId").GetGuid().Should().Be(promotion.PromotionId);
            foundPromotion.GetProperty("name").GetString().Should().Be(TestData.NewPromotions.NewPromotion1Name);
            foundPromotion.GetProperty("discountPercent").GetDecimal().Should().Be(TestData.NewPromotions.NewPromotion1DiscountPercent);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAllPromotions_Should_ReturnAllProperties_WhenPromotionsExist] Response: {jsonString}");
            throw;
        }
    }
}
