namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class ActivatePromotionTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_ActivatePromotion_Should_Return200_WhenPromotionExists()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync("Неактивная акция", 10, isActive: false);

        var response = await Client.PostAsync($"/promotions/{promotion.PromotionId}/activate", null);
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

        var response = await Client.PostAsync("/promotions/9999/activate", null);
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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Endpoint_ActivatePromotion_Should_Return422_WhenPromotionIdIsInvalid(int invalidId)
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.PostAsync($"/promotions/{invalidId}/activate", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ActivatePromotion_Should_Return422_WhenPromotionIdIsInvalid({invalidId})] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ActivatePromotion_Should_ActuallyActivatePromotion_WhenCalled()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync("Акция для активации", 10, isActive: false);

        await Client.PostAsync($"/promotions/{promotion.PromotionId}/activate", null);

        var response = await Client.GetAsync($"/promotions/{promotion.PromotionId}");
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
