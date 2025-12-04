namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class GetAllPromotionsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetAllPromotions_Should_Return200_WhenPromotionsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedPromotionAsync("Акция 1", 10);
        await SeedPromotionAsync("Акция 2", 20);
        await SeedPromotionAsync("Акция 3", 30);

        var response = await Client.GetAsync("/promotions");
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

        var response = await Client.GetAsync("/promotions");
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
        var promotion = await SeedPromotionAsync("Специальная акция", 50);

        var response = await Client.GetAsync("/promotions");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var promotions = json.GetProperty("promotions");
            var foundPromotion = promotions.EnumerateArray().FirstOrDefault(p => p.GetProperty("name").GetString() == "Специальная акция");

            foundPromotion.ValueKind.Should().NotBe(JsonValueKind.Undefined);
            foundPromotion.GetProperty("promotionId").GetInt32().Should().Be(promotion.PromotionId);
            foundPromotion.GetProperty("name").GetString().Should().Be("Специальная акция");
            foundPromotion.GetProperty("discountPercent").GetDecimal().Should().Be(50);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAllPromotions_Should_ReturnAllProperties_WhenPromotionsExist] Response: {jsonString}");
            throw;
        }
    }
}
