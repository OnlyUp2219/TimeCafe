using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Venue.TimeCafe.Domain.Enums;
using Venue.TimeCafe.Test.Integration.Helpers;

namespace Venue.TimeCafe.Test.Integration.Repository;

public class PromotionsCacheTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Cache_ShouldBeInvalidated_OnCreateAndUpdateEndpoints()
    {
        // Arrange
        await ClearDatabaseAndCacheAsync();
        
        // 1. Получить данные (Initial GET)
        var initialGetResponse = await Client.GetAsync("/venue/promotions/active");
        if (initialGetResponse.StatusCode == HttpStatusCode.NotFound)
            initialGetResponse = await Client.GetAsync("/venue/promotions"); // Fallback if no active endpoint

        initialGetResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        var initialCount = 0;
        if (initialGetResponse.IsSuccessStatusCode)
        {
            var initialJsonStr = await initialGetResponse.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(initialJsonStr).RootElement;
            var initialItems = root.ValueKind == JsonValueKind.Array 
                ? root.EnumerateArray().ToList() 
                : root.GetProperty("items").EnumerateArray().ToList();
            initialCount = initialItems.Count;
        }

        // 2. Создание (POST)
        var createPayload = new
        {
            name = "New Cached Promotion",
            description = "Promo description",
            discountPercent = 10.0m,
            validFrom = DateTimeOffset.UtcNow.AddDays(-1),
            validTo = DateTimeOffset.UtcNow.AddDays(10),
            type = (int)PromotionType.Global,
            isActive = true
        };

        var createResponse = await Client.PostAsJsonAsync("/venue/promotions", createPayload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createJsonStr = await createResponse.Content.ReadAsStringAsync();
        var createdPromotionId = JsonDocument.Parse(createJsonStr).RootElement.GetProperty("promotionId").GetString();
        createdPromotionId.Should().NotBeNullOrEmpty();

        // 3. Получить данные (GET after POST)
        var getAfterCreateResponse = await Client.GetAsync("/venue/promotions");
        getAfterCreateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterCreateJsonStr = await getAfterCreateResponse.Content.ReadAsStringAsync();
        var rootAfterCreate = JsonDocument.Parse(getAfterCreateJsonStr).RootElement;
        var itemsAfterCreate = rootAfterCreate.ValueKind == JsonValueKind.Array 
            ? rootAfterCreate.EnumerateArray().ToList() 
            : rootAfterCreate.GetProperty("items").EnumerateArray().ToList();
        
        itemsAfterCreate.Count.Should().BeGreaterThan(initialCount, "Новая акция должна появиться в списке, кэш инвалидирован");
        var createdItem = itemsAfterCreate.FirstOrDefault(x => x.GetProperty("promotionId").GetString() == createdPromotionId);
        createdItem.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        createdItem.GetProperty("name").GetString().Should().Be("New Cached Promotion");

        // 4. Обновление (PUT)
        var updatePayload = new
        {
            name = "Updated Cached Promotion",
            description = "Promo description updated",
            discountPercent = 15.0m,
            validFrom = DateTimeOffset.UtcNow.AddDays(-1),
            validTo = DateTimeOffset.UtcNow.AddDays(10),
            type = (int)PromotionType.Global,
            isActive = true
        };

        var updateResponse = await Client.PutAsJsonAsync($"/venue/promotions/{createdPromotionId}", updatePayload);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Получить данные (GET after PUT)
        var getAfterUpdateResponse = await Client.GetAsync("/venue/promotions");
        getAfterUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterUpdateJsonStr = await getAfterUpdateResponse.Content.ReadAsStringAsync();
        var rootAfterUpdate = JsonDocument.Parse(getAfterUpdateJsonStr).RootElement;
        var itemsAfterUpdate = rootAfterUpdate.ValueKind == JsonValueKind.Array 
            ? rootAfterUpdate.EnumerateArray().ToList() 
            : rootAfterUpdate.GetProperty("items").EnumerateArray().ToList();
        
        var updatedItem = itemsAfterUpdate.FirstOrDefault(x => x.GetProperty("promotionId").GetString() == createdPromotionId);
        updatedItem.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        updatedItem.GetProperty("name").GetString().Should().Be("Updated Cached Promotion", "Акция должна была обновиться, кэш инвалидирован");
    }
}
