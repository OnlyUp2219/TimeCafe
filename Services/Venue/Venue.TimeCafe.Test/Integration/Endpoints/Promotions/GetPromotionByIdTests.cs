namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class GetPromotionByIdTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetPromotionById_Should_Return200_WhenPromotionExists()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, (int)TestData.ExistingPromotions.Promotion1DiscountPercent);

        var response = await Client.GetAsync($"/venue/promotions/{promotion.PromotionId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;

            json.GetProperty("promotionId").GetGuid().Should().Be(promotion.PromotionId);
            json.GetProperty("name").GetString().Should().Be(TestData.ExistingPromotions.Promotion1Name);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetPromotionById_Should_Return200_WhenPromotionExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetPromotionById_Should_Return404_WhenPromotionNotFound()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.GetAsync($"/venue/promotions/{TestData.NonExistingIds.NonExistingPromotionId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetPromotionById_Should_Return404_WhenPromotionNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetPromotionById_Should_Return404_WhenPromotionIdIsEmpty()
    {
        var response = await Client.GetAsync($"/venue/promotions/{Guid.Empty}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var json = JsonDocument.Parse(jsonString).RootElement;
            if (json.TryGetProperty("code", out var code))
                code.GetString().Should().Be("PromotionNotFound");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetPromotionById_Should_Return404_WhenPromotionIdIsEmpty] Response: {jsonString}");
            throw;
        }
    }
    [Fact]
    public async Task Endpoint_GetPromotionById_Should_ReturnAllProperties_WhenPromotionExists()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion3Name, (int)TestData.ExistingPromotions.Promotion3DiscountPercent);

        var response = await Client.GetAsync($"/venue/promotions/{promotion.PromotionId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;

            json.GetProperty("promotionId").GetGuid().Should().Be(promotion.PromotionId);
            json.GetProperty("name").GetString().Should().Be(TestData.ExistingPromotions.Promotion3Name);
            json.GetProperty("discountPercent").GetDecimal().Should().Be(TestData.ExistingPromotions.Promotion3DiscountPercent);
            json.TryGetProperty("validFrom", out _).Should().BeTrue();
            json.TryGetProperty("validTo", out _).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetPromotionById_Should_ReturnAllProperties_WhenPromotionExists] Response: {jsonString}");
            throw;
        }
    }
}













