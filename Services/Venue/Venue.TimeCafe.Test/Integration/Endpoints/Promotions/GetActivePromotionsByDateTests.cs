namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class GetActivePromotionsByDateTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetActivePromotionsByDate_Should_Return200_WhenPromotionsExistForDate()
    {
        await ClearDatabaseAndCacheAsync();
        var targetDate = DateTimeOffset.UtcNow;
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, (int)TestData.ExistingPromotions.Promotion1DiscountPercent, isActive: true, validFrom: targetDate.AddDays(-1), validTo: targetDate.AddDays(1));
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, (int)TestData.ExistingPromotions.Promotion2DiscountPercent, isActive: true, validFrom: targetDate.AddDays(-10), validTo: targetDate.AddDays(-5));

        var response = await Client.GetAsync($"/venue/promotions/active/{targetDate:yyyy-MM-ddTHH:mm:ss}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("promotions", out var promotions).Should().BeTrue();
            promotions.ValueKind.Should().Be(JsonValueKind.Array);
            promotions.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActivePromotionsByDate_Should_Return200_WhenPromotionsExistForDate] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetActivePromotionsByDate_Should_Return200_WhenNoPromotionsExistForDate()
    {
        await ClearDatabaseAndCacheAsync();
        var futureDate = DateTimeOffset.UtcNow.AddDays(100);
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, (int)TestData.ExistingPromotions.Promotion1DiscountPercent, isActive: true, validFrom: TestData.DateTimeData.GetValidFromDate(), validTo: TestData.DateTimeData.GetValidToDate());

        var response = await Client.GetAsync($"/venue/promotions/active/{futureDate:yyyy-MM-ddTHH:mm:ss}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("promotions", out var promotions).Should().BeTrue();
            promotions.ValueKind.Should().Be(JsonValueKind.Array);
            promotions.GetArrayLength().Should().Be(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActivePromotionsByDate_Should_Return200_WhenNoPromotionsExistForDate] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetActivePromotionsByDate_Should_OnlyReturnPromotionsValidForDate()
    {
        await ClearDatabaseAndCacheAsync();
        var targetDate = DateTimeOffset.UtcNow;
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion1Name, (int)TestData.ExistingPromotions.Promotion1DiscountPercent, isActive: true, validFrom: targetDate.AddDays(-1), validTo: targetDate.AddDays(1));
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion2Name, (int)TestData.ExistingPromotions.Promotion2DiscountPercent, isActive: true, validFrom: targetDate.AddDays(-10), validTo: targetDate.AddDays(-5));
        await SeedPromotionAsync(TestData.ExistingPromotions.Promotion3Name, (int)TestData.ExistingPromotions.Promotion3DiscountPercent, isActive: true, validFrom: targetDate.AddDays(5), validTo: targetDate.AddDays(10));

        var response = await Client.GetAsync($"/venue/promotions/active/{targetDate:yyyy-MM-ddTHH:mm:ss}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var promotions = json.GetProperty("promotions");
            foreach (var promotion in promotions.EnumerateArray())
            {
                var validFrom = promotion.GetProperty("validFrom").GetDateTime();
                var validTo = promotion.GetProperty("validTo").GetDateTime();
                validFrom.Should().BeOnOrBefore(targetDate.UtcDateTime);
                validTo.Should().BeOnOrAfter(targetDate.UtcDateTime);
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActivePromotionsByDate_Should_OnlyReturnPromotionsValidForDate] Response: {jsonString}");
            throw;
        }
    }
}
