namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class GetPromotionByIdTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetPromotionById_Should_Return200_WhenPromotionExists()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, (int)TestData.ExistingPromotions.Promotion1DiscountPercent);

        var response = await Client.GetAsync($"/promotions/{promotion.PromotionId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("promotion", out var promotionJson).Should().BeTrue();
            promotionJson.GetProperty("promotionId").GetGuid().Should().Be(promotion.PromotionId);
            promotionJson.GetProperty("name").GetString().Should().Be(TestData.ExistingPromotions.Promotion1Name);
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

        var response = await Client.GetAsync($"/promotions/{TestData.NonExistingIds.NonExistingPromotionId}");
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
        await ClearDatabaseAndCacheAsync();

        var response = await Client.GetAsync($"/promotions//");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("123-invalid")]
    public async Task Endpoint_GetPromotionById_Should_Return422_WhenPromotionIdIsInvalid(string invalidId)
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.GetAsync($"/promotions/{invalidId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetPromotionById_Should_Return422_WhenPromotionIdIsInvalid({invalidId})] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetPromotionById_Should_ReturnAllProperties_WhenPromotionExists()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion3Name, (int)TestData.ExistingPromotions.Promotion3DiscountPercent);

        var response = await Client.GetAsync($"/promotions/{promotion.PromotionId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var promotionJson = json.GetProperty("promotion");
            promotionJson.GetProperty("promotionId").GetGuid().Should().Be(promotion.PromotionId);
            promotionJson.GetProperty("name").GetString().Should().Be(TestData.ExistingPromotions.Promotion3Name);
            promotionJson.GetProperty("discountPercent").GetDecimal().Should().Be(TestData.ExistingPromotions.Promotion3DiscountPercent);
            promotionJson.TryGetProperty("validFrom", out _).Should().BeTrue();
            promotionJson.TryGetProperty("validTo", out _).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetPromotionById_Should_ReturnAllProperties_WhenPromotionExists] Response: {jsonString}");
            throw;
        }
    }
}
