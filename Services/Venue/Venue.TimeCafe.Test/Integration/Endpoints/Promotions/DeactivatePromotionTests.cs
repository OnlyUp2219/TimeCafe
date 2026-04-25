namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class DeactivatePromotionTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeactivatePromotion_Should_Return204_WhenPromotionExists()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, (int)TestData.ExistingPromotions.Promotion1DiscountPercent, isActive: true);

        var response = await Client.PostAsync($"/venue/promotions/{promotion.PromotionId}/deactivate", null);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Endpoint_DeactivatePromotion_Should_Return404_WhenPromotionNotFound()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.PostAsync($"/venue/promotions/{TestData.NonExistingIds.NonExistingPromotionId}/deactivate", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Endpoint_DeactivatePromotion_Should_Return422_WhenPromotionIdIsEmpty()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.PostAsync($"/venue/promotions/{Guid.Empty}/deactivate", null);
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Endpoint_DeactivatePromotion_Should_ActuallyDeactivatePromotion_WhenCalled()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, (int)TestData.ExistingPromotions.Promotion2DiscountPercent, isActive: true);

        var deactivateResponse = await Client.PostAsync($"/venue/promotions/{promotion.PromotionId}/deactivate", null);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await Client.GetAsync($"/venue/promotions/{promotion.PromotionId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.GetProperty("isActive").GetBoolean().Should().BeFalse();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeactivatePromotion_Should_ActuallyDeactivatePromotion_WhenCalled] Response: {jsonString}");
            throw;
        }
    }
}





