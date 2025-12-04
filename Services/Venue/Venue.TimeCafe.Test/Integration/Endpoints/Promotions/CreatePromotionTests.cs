namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class CreatePromotionTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_CreatePromotion_Should_Return201_WhenValid()
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new
        {
            name = "Новая акция",
            description = "Описание акции",
            discountPercent = 15m,
            validFrom = DateTime.UtcNow,
            validTo = DateTime.UtcNow.AddDays(7),
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out _).Should().BeTrue();
            json.TryGetProperty("promotion", out var promotion).Should().BeTrue();
            promotion.GetProperty("name").GetString().Should().Be("Новая акция");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreatePromotion_Should_Return201_WhenValid] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Endpoint_CreatePromotion_Should_Return422_WhenNameIsInvalid(string invalidName)
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new
        {
            name = invalidName,
            description = "Описание",
            discountPercent = 10m,
            validFrom = DateTime.UtcNow,
            validTo = DateTime.UtcNow.AddDays(7),
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreatePromotion_Should_Return422_WhenNameIsInvalid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreatePromotion_Should_Return422_WhenNameExceedsMaxLength()
    {
        await ClearDatabaseAndCacheAsync();
        var longName = new string('A', 201);
        var payload = new
        {
            name = longName,
            description = "Описание",
            discountPercent = 10m,
            validFrom = DateTime.UtcNow,
            validTo = DateTime.UtcNow.AddDays(7),
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreatePromotion_Should_Return422_WhenNameExceedsMaxLength] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task Endpoint_CreatePromotion_Should_Return422_WhenDiscountPercentIsInvalid(decimal invalidDiscount)
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new
        {
            name = "Акция",
            description = "Описание",
            discountPercent = invalidDiscount,
            validFrom = DateTime.UtcNow,
            validTo = DateTime.UtcNow.AddDays(7),
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreatePromotion_Should_Return422_WhenDiscountPercentIsInvalid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreatePromotion_Should_Return422_WhenValidFromIsAfterValidTo()
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new
        {
            name = "Акция",
            description = "Описание",
            discountPercent = 10m,
            validFrom = DateTime.UtcNow.AddDays(7),
            validTo = DateTime.UtcNow,
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreatePromotion_Should_Return422_WhenValidFromIsAfterValidTo] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreatePromotion_Should_ReturnPromotionWithId_WhenCreated()
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new
        {
            name = "Акция с ID",
            description = "Описание",
            discountPercent = 20m,
            validFrom = DateTime.UtcNow,
            validTo = DateTime.UtcNow.AddDays(7),
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var promotion = json.GetProperty("promotion");
            promotion.GetProperty("promotionId").GetInt32().Should().BeGreaterThan(0);
            promotion.GetProperty("name").GetString().Should().Be("Акция с ID");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreatePromotion_Should_ReturnPromotionWithId_WhenCreated] Response: {jsonString}");
            throw;
        }
    }
}
