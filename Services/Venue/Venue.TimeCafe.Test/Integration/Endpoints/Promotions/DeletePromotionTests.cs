namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class DeletePromotionTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeletePromotion_Should_Return200_WhenPromotionExists()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, (int)TestData.ExistingPromotions.Promotion1DiscountPercent);

        var response = await Client.DeleteAsync($"/promotions/{promotion.PromotionId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out _).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeletePromotion_Should_Return200_WhenPromotionExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeletePromotion_Should_Return404_WhenPromotionNotFound()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.DeleteAsync($"/promotions/{TestData.NonExistingIds.NonExistingPromotionId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeletePromotion_Should_Return404_WhenPromotionNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeletePromotion_Should_ActuallyRemoveFromDatabase_WhenDeleted()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, (int)TestData.ExistingPromotions.Promotion2DiscountPercent);

        var deleteResponse = await Client.DeleteAsync($"/promotions/{promotion.PromotionId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await Client.GetAsync($"/promotions/{promotion.PromotionId}");
        var jsonString = await getResponse.Content.ReadAsStringAsync();
        try
        {
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeletePromotion_Should_ActuallyRemoveFromDatabase_WhenDeleted] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeletePromotion_Should_Return404_WhenPromotionIdIsEmpty()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.DeleteAsync($"/promotions//");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("123-invalid")]
    public async Task Endpoint_DeletePromotion_Should_Return422_WhenPromotionIdIsInvalid(string invalidId)
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.DeleteAsync($"/promotions/{invalidId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeletePromotion_Should_Return422_WhenPromotionIdIsInvalid({invalidId})] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeletePromotion_Should_NotAffectOtherPromotions_WhenOneIsDeleted()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion1 = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, (int)TestData.ExistingPromotions.Promotion1DiscountPercent);
        var promotion2 = await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, (int)TestData.ExistingPromotions.Promotion2DiscountPercent);

        await Client.DeleteAsync($"/promotions/{promotion1.PromotionId}");

        var response = await Client.GetAsync($"/promotions/{promotion2.PromotionId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.GetProperty("promotion").GetProperty("name").GetString().Should().Be(TestData.ExistingPromotions.Promotion2Name);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeletePromotion_Should_NotAffectOtherPromotions_WhenOneIsDeleted] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeletePromotion_Should_ReturnSuccessMessage_WhenPromotionDeleted()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync("Акция с сообщением", 10);

        var response = await Client.DeleteAsync($"/promotions/{promotion.PromotionId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString().Should().NotBeNullOrEmpty();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeletePromotion_Should_ReturnSuccessMessage_WhenPromotionDeleted] Response: {jsonString}");
            throw;
        }
    }
}
