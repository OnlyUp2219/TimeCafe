namespace Venue.TimeCafe.Test.Integration.Endpoints.Promotions;

public class GetActivePromotionsByDateTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetActivePromotionsByDate_Should_Return200_WhenPromotionsExistForDate()
    {
        await ClearDatabaseAndCacheAsync();
        var targetDate = DateTime.UtcNow;
        await SeedPromotionAsync("Актуальная акция", 10, validFrom: targetDate.AddDays(-1), validTo: targetDate.AddDays(1), isActive: true);
        await SeedPromotionAsync("Прошедшая акция", 20, validFrom: targetDate.AddDays(-10), validTo: targetDate.AddDays(-5), isActive: true);

        var response = await Client.GetAsync($"/promotions/active/{targetDate:yyyy-MM-ddTHH:mm:ss}");
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
        var futureDate = DateTime.UtcNow.AddDays(100);
        await SeedPromotionAsync("Текущая акция", 10, validFrom: DateTime.UtcNow.AddDays(-1), validTo: DateTime.UtcNow.AddDays(1), isActive: true);

        var response = await Client.GetAsync($"/promotions/active/{futureDate:yyyy-MM-ddTHH:mm:ss}");
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
        var targetDate = DateTime.UtcNow;
        await SeedPromotionAsync("Актуальная", 10, validFrom: targetDate.AddDays(-1), validTo: targetDate.AddDays(1), isActive: true);
        await SeedPromotionAsync("Прошедшая", 20, validFrom: targetDate.AddDays(-10), validTo: targetDate.AddDays(-5), isActive: true);
        await SeedPromotionAsync("Будущая", 30, validFrom: targetDate.AddDays(5), validTo: targetDate.AddDays(10), isActive: true);

        var response = await Client.GetAsync($"/promotions/active/{targetDate:yyyy-MM-ddTHH:mm:ss}");
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
                validFrom.Should().BeOnOrBefore(targetDate);
                validTo.Should().BeOnOrAfter(targetDate);
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActivePromotionsByDate_Should_OnlyReturnPromotionsValidForDate] Response: {jsonString}");
            throw;
        }
    }
}
