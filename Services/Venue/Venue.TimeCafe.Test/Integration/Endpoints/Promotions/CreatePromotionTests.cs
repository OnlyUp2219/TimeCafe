namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class CreatePromotionTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_CreatePromotion_Should_Return201_WhenValid()
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new
        {
            name = TestData.NewPromotions.NewPromotion1Name,
            description = TestData.NewPromotions.NewPromotion1Description,
            DiscountPercent = TestData.NewPromotions.NewPromotion1DiscountPercent,
            validFrom = TestData.DateTimeData.GetValidFromDate(),
            validTo = TestData.DateTimeData.GetValidToDate(),
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/venue/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;


            json.GetProperty("name").GetString().Should().Be(TestData.NewPromotions.NewPromotion1Name);
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
    public async Task Endpoint_CreatePromotion_Should_Return422_WhenNameIsInvalid(string? invalidName)
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new
        {
            name = invalidName,
            description = TestData.DefaultValues.DefaultPromotionDescription,
            DiscountPercent = TestData.DefaultValues.DefaultDiscountPercent,
            validFrom = TestData.DateTimeData.GetValidFromDate(),
            validTo = TestData.DateTimeData.GetValidToDate(),
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/venue/promotions", payload);
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
        var longName = TestData.ValidationBoundaries.VeryLongString;
        var payload = new
        {
            name = longName,
            description = TestData.DefaultValues.DefaultPromotionDescription,
            DiscountPercent = TestData.DefaultValues.DefaultDiscountPercent,
            validFrom = TestData.DateTimeData.GetValidFromDate(),
            validTo = TestData.DateTimeData.GetValidToDate(),
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/venue/promotions", payload);
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
            name = TestData.ExistingPromotions.Promotion1Name,
            description = TestData.DefaultValues.DefaultPromotionDescription,
            DiscountPercent = invalidDiscount,
            validFrom = TestData.DateTimeData.GetValidFromDate(),
            validTo = TestData.DateTimeData.GetValidToDate(),
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/venue/promotions", payload);
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
            name = TestData.ExistingPromotions.Promotion1Name,
            description = TestData.DefaultValues.DefaultPromotionDescription,
            DiscountPercent = TestData.DefaultValues.DefaultDiscountPercent,
            validFrom = TestData.DateTimeData.GetValidToDate(),
            validTo = TestData.DateTimeData.GetValidFromDate(),
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/venue/promotions", payload);
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
            name = TestData.NewPromotions.NewPromotion2Name,
            description = TestData.NewPromotions.NewPromotion2Description,
            DiscountPercent = TestData.NewPromotions.NewPromotion2DiscountPercent,
            validFrom = TestData.DateTimeData.GetValidFromDate(),
            validTo = TestData.DateTimeData.GetValidToDate(),
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/venue/promotions", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var promotion = json;
            json.GetProperty("promotionId").GetGuid().Should().NotBeEmpty();
            json.GetProperty("name").GetString().Should().Be(TestData.NewPromotions.NewPromotion2Name);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreatePromotion_Should_ReturnPromotionWithId_WhenCreated] Response: {jsonString}");
            throw;
        }
    }
}













