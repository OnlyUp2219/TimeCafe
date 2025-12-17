namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class GetActivePromotionsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetActivePromotions_Should_Return200_WhenActivePromotionsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, (int)TestData.ExistingPromotions.Promotion1DiscountPercent, isActive: true);
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, (int)TestData.ExistingPromotions.Promotion2DiscountPercent, isActive: false);
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion3Name, (int)TestData.ExistingPromotions.Promotion3DiscountPercent, isActive: true);

        var response = await Client.GetAsync("/promotions/active");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("promotions", out var promotions).Should().BeTrue();
            promotions.ValueKind.Should().Be(JsonValueKind.Array);
            var activePromotions = promotions.EnumerateArray().Where(p => p.GetProperty("isActive").GetBoolean()).ToList();
            activePromotions.Should().HaveCountGreaterThanOrEqualTo(2);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActivePromotions_Should_Return200_WhenActivePromotionsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetActivePromotions_Should_Return200_WhenNoActivePromotionsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedPromotionAsync(TestData.NewPromotions.NewPromotion1Name, (int)TestData.NewPromotions.NewPromotion1DiscountPercent, isActive: false);
        await SeedPromotionAsync(TestData.NewPromotions.NewPromotion2Name, (int)TestData.NewPromotions.NewPromotion2DiscountPercent, isActive: false);

        var response = await Client.GetAsync("/promotions/active");
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
            Console.WriteLine($"[Endpoint_GetActivePromotions_Should_Return200_WhenNoActivePromotionsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetActivePromotions_Should_OnlyReturnActivePromotions_WhenBothExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, (int)TestData.ExistingPromotions.Promotion1DiscountPercent, isActive: true);
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, (int)TestData.ExistingPromotions.Promotion2DiscountPercent, isActive: false);

        var response = await Client.GetAsync("/promotions/active");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var promotions = json.GetProperty("promotions");
            foreach (var promotion in promotions.EnumerateArray())
            {
                promotion.GetProperty("isActive").GetBoolean().Should().BeTrue();
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActivePromotions_Should_OnlyReturnActivePromotions_WhenBothExist] Response: {jsonString}");
            throw;
        }
    }
}
