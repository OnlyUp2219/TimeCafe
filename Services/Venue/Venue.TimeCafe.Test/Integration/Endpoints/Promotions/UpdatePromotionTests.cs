namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class UpdatePromotionTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_UpdatePromotion_Should_Return200_WhenPromotionExists()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync("Старое название", 10);
        var payload = new
        {
            promotionId = promotion.PromotionId,
            name = "Новое название",
            description = promotion.Description,
            discountPercent = promotion.DiscountPercent,
            validFrom = promotion.ValidFrom,
            validTo = promotion.ValidTo,
            isActive = promotion.IsActive
        };

        var response = await Client.PutAsJsonAsync("/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out _).Should().BeTrue();
            json.TryGetProperty("promotion", out var promotionJson).Should().BeTrue();
            promotionJson.GetProperty("name").GetString().Should().Be("Новое название");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdatePromotion_Should_Return200_WhenPromotionExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UpdatePromotion_Should_Return404_WhenPromotionNotFound()
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new
        {
            promotionId = 9999,
            name = "Несуществующая",
            description = "Описание",
            discountPercent = 10m,
            validFrom = DateTime.UtcNow,
            validTo = DateTime.UtcNow.AddDays(7),
            isActive = true
        };

        var response = await Client.PutAsJsonAsync("/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdatePromotion_Should_Return404_WhenPromotionNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UpdatePromotion_Should_UpdateOnlyChangedFields_WhenPartialUpdateProvided()
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync("Название до обновления", 10);
        var payload = new
        {
            promotionId = promotion.PromotionId,
            name = "Название после обновления",
            description = promotion.Description,
            discountPercent = promotion.DiscountPercent,
            validFrom = promotion.ValidFrom,
            validTo = promotion.ValidTo,
            isActive = promotion.IsActive
        };

        var response = await Client.PutAsJsonAsync("/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var updatedPromotion = json.GetProperty("promotion");
            updatedPromotion.GetProperty("name").GetString().Should().Be("Название после обновления");
            updatedPromotion.GetProperty("discountPercent").GetDecimal().Should().Be(10m);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdatePromotion_Should_UpdateOnlyChangedFields_WhenPartialUpdateProvided] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Endpoint_UpdatePromotion_Should_Return422_WhenNameIsInvalid(string invalidName)
    {
        await ClearDatabaseAndCacheAsync();
        var promotion = await SeedPromotionAsync("Акция", 10);
        var payload = new
        {
            promotionId = promotion.PromotionId,
            name = invalidName,
            description = "Описание",
            discountPercent = 10m,
            validFrom = DateTime.UtcNow,
            validTo = DateTime.UtcNow.AddDays(7),
            isActive = true
        };

        var response = await Client.PutAsJsonAsync("/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdatePromotion_Should_Return422_WhenNameIsInvalid] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Endpoint_UpdatePromotion_Should_Return422_WhenPromotionIdIsInvalid(int invalidId)
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new
        {
            promotionId = invalidId,
            name = "Акция",
            description = "Описание",
            discountPercent = 10m,
            validFrom = DateTime.UtcNow,
            validTo = DateTime.UtcNow.AddDays(7),
            isActive = true
        };

        var response = await Client.PutAsJsonAsync("/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdatePromotion_Should_Return422_WhenPromotionIdIsInvalid({invalidId})] Response: {jsonString}");
            throw;
        }
    }
}
