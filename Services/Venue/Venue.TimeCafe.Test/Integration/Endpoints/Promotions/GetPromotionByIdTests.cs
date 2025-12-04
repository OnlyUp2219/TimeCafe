namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class GetPromotionByIdTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetPromotionById_Should_Return200_WhenPromotionExists()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync("Существующая акция", 15);

        var response = await Client.GetAsync($"/promotions/{promotion.PromotionId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("promotion", out var promotionJson).Should().BeTrue();
            promotionJson.GetProperty("promotionId").GetInt32().Should().Be(promotion.PromotionId);
            promotionJson.GetProperty("name").GetString().Should().Be("Существующая акция");
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

        var response = await Client.GetAsync("/promotions/9999");
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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task Endpoint_GetPromotionById_Should_Return422_WhenPromotionIdIsInvalid(int invalidId)
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
        var promotion = await SeedPromotionAsync("Детальная акция", 25);

        var response = await Client.GetAsync($"/promotions/{promotion.PromotionId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var promotionJson = json.GetProperty("promotion");
            promotionJson.GetProperty("promotionId").GetInt32().Should().Be(promotion.PromotionId);
            promotionJson.GetProperty("name").GetString().Should().Be("Детальная акция");
            promotionJson.GetProperty("discountPercent").GetDecimal().Should().Be(25);
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
