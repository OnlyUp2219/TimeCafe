namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class ActivatePromotionTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_ActivatePromotion_Should_Return200_WhenPromotionExists()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, (int)TestData.ExistingPromotions.Promotion1DiscountPercent, isActive: false);

        var response = await Client.PostAsync($"/venue/promotions/{promotion.PromotionId}/activate", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out _).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ActivatePromotion_Should_Return200_WhenPromotionExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ActivatePromotion_Should_Return404_WhenPromotionNotFound()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.PostAsync($"/venue/promotions/{TestData.NonExistingIds.NonExistingPromotionId}/activate", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ActivatePromotion_Should_Return404_WhenPromotionNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ActivatePromotion_Should_Return404_WhenPromotionIdIsEmpty()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.PostAsync("/venue/promotions//activate", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Endpoint_ActivatePromotion_Should_Return422_WhenPromotionIdIsEmpty()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.PostAsync($"/venue/promotions/{Guid.Empty}/activate", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ActivatePromotion_Should_Return422_WhenPromotionIdIsEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ActivatePromotion_Should_ActuallyActivatePromotion_WhenCalled()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, (int)TestData.ExistingPromotions.Promotion2DiscountPercent, isActive: false);

        await Client.PostAsync($"/venue/promotions/{promotion.PromotionId}/activate", null);

        var response = await Client.GetAsync($"/venue/promotions/{promotion.PromotionId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.GetProperty("promotion").GetProperty("isActive").GetBoolean().Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ActivatePromotion_Should_ActuallyActivatePromotion_WhenCalled] Response: {jsonString}");
            throw;
        }
    }
}
