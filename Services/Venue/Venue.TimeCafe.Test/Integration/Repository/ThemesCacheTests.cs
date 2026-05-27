using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Venue.TimeCafe.Test.Integration.Helpers;

namespace Venue.TimeCafe.Test.Integration.Repository;

public class ThemesCacheTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Cache_ShouldBeInvalidated_OnCreateAndUpdateEndpoints()
    {
        // Arrange
        await ClearDatabaseAndCacheAsync();
        
        // 1. Получить данные (Initial GET)
        var initialGetResponse = await Client.GetAsync("/venue/themes");
        initialGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var initialJsonStr = await initialGetResponse.Content.ReadAsStringAsync();
        // Ответ может быть в формате PagedResponse, проверим
        var root = JsonDocument.Parse(initialJsonStr).RootElement;
        
        // В Venue API GetAllThemes обычно не пагинированный, либо список, либо PagedResult.
        // Допустим, это массив.
        List<JsonElement> initialItems;
        if (root.ValueKind == JsonValueKind.Array)
        {
            initialItems = root.EnumerateArray().ToList();
        }
        else
        {
            initialItems = root.GetProperty("items").EnumerateArray().ToList();
        }
        
        var initialCount = initialItems.Count;

        // 2. Создание (POST)
        var createPayload = new
        {
            name = "New Cached Theme",
            emoji = "🎨",
            colors = "{\"primary\":\"#ffffff\"}"
        };

        var createResponse = await Client.PostAsJsonAsync("/venue/themes", createPayload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createJsonStr = await createResponse.Content.ReadAsStringAsync();
        var createdThemeId = JsonDocument.Parse(createJsonStr).RootElement.GetProperty("themeId").GetString();
        createdThemeId.Should().NotBeNullOrEmpty();

        // 3. Получить данные (GET after POST)
        var getAfterCreateResponse = await Client.GetAsync("/venue/themes");
        getAfterCreateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterCreateJsonStr = await getAfterCreateResponse.Content.ReadAsStringAsync();
        root = JsonDocument.Parse(getAfterCreateJsonStr).RootElement;
        var itemsAfterCreate = root.ValueKind == JsonValueKind.Array 
            ? root.EnumerateArray().ToList() 
            : root.GetProperty("items").EnumerateArray().ToList();
        
        itemsAfterCreate.Count.Should().Be(initialCount + 1, "Новая тема должна появиться в списке, кэш инвалидирован");
        var createdItem = itemsAfterCreate.FirstOrDefault(x => x.GetProperty("themeId").GetString() == createdThemeId);
        createdItem.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        createdItem.GetProperty("name").GetString().Should().Be("New Cached Theme");

        // 4. Обновление (PUT)
        var updatePayload = new
        {
            name = "Updated Cached Theme",
            emoji = "🖌️",
            colors = "{\"primary\":\"#000000\"}"
        };

        var updateResponse = await Client.PutAsJsonAsync($"/venue/themes/{createdThemeId}", updatePayload);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Получить данные (GET after PUT)
        var getAfterUpdateResponse = await Client.GetAsync("/venue/themes");
        getAfterUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterUpdateJsonStr = await getAfterUpdateResponse.Content.ReadAsStringAsync();
        root = JsonDocument.Parse(getAfterUpdateJsonStr).RootElement;
        var itemsAfterUpdate = root.ValueKind == JsonValueKind.Array 
            ? root.EnumerateArray().ToList() 
            : root.GetProperty("items").EnumerateArray().ToList();
        
        itemsAfterUpdate.Count.Should().Be(initialCount + 1);
        var updatedItem = itemsAfterUpdate.FirstOrDefault(x => x.GetProperty("themeId").GetString() == createdThemeId);
        updatedItem.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        updatedItem.GetProperty("name").GetString().Should().Be("Updated Cached Theme", "Тема должна была обновиться, кэш инвалидирован");
    }
}
