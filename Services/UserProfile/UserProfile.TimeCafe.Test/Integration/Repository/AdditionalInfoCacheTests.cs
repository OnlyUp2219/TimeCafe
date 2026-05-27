using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using UserProfile.TimeCafe.Test.Integration.Helpers;

namespace UserProfile.TimeCafe.Test.Integration.Repository;

public class AdditionalInfoCacheTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Cache_ShouldBeInvalidated_OnCreateAndUpdateEndpoints()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Сначала создадим профиль, так как AdditionalInfo может требовать существующий профиль (Foreign Key)
        var createProfilePayload = new
        {
            userId = userId,
            firstName = "Test",
            lastName = "User",
            email = "test@example.com",
            phone = "+1234567890",
            gender = 1,
            birthDate = "1990-01-01"
        };
        await Client.PostAsJsonAsync("/userprofile/profiles", createProfilePayload);

        // 1. Получить данные (Initial GET)
        var initialGetResponse = await Client.GetAsync($"/userprofile/profiles/{userId}/infos");
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
            userId = userId,
            infoText = "Reading",
            createdBy = "tester"
        };

        var createResponse = await Client.PostAsJsonAsync("/userprofile/infos", createPayload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createJsonStr = await createResponse.Content.ReadAsStringAsync();
        var createdInfoId = JsonDocument.Parse(createJsonStr).RootElement.GetProperty("infoId").GetString();
        createdInfoId.Should().NotBeNullOrEmpty();

        // 3. Получить данные (GET after POST)
        var getAfterCreateResponse = await Client.GetAsync($"/userprofile/profiles/{userId}/infos");
        getAfterCreateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterCreateJsonStr = await getAfterCreateResponse.Content.ReadAsStringAsync();
        var rootAfterCreate = JsonDocument.Parse(getAfterCreateJsonStr).RootElement;
        var itemsAfterCreate = rootAfterCreate.ValueKind == JsonValueKind.Array 
            ? rootAfterCreate.EnumerateArray().ToList() 
            : rootAfterCreate.GetProperty("items").EnumerateArray().ToList();
        
        itemsAfterCreate.Count.Should().BeGreaterThan(initialCount, "Новая доп. информация должна появиться в списке, кэш инвалидирован");
        var createdItem = itemsAfterCreate.FirstOrDefault(x => x.GetProperty("infoId").GetString() == createdInfoId);
        createdItem.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        createdItem.GetProperty("infoText").GetString().Should().Be("Reading");

        // 4. Обновление (PUT)
        var updatePayload = new
        {
            userId = userId,
            infoText = "Gaming",
            createdBy = "tester2"
        };

        var updateResponse = await Client.PutAsJsonAsync($"/userprofile/infos/{createdInfoId}", updatePayload);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 5. Получить данные (GET after PUT)
        var getAfterUpdateResponse = await Client.GetAsync($"/userprofile/profiles/{userId}/infos");
        getAfterUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterUpdateJsonStr = await getAfterUpdateResponse.Content.ReadAsStringAsync();
        var rootAfterUpdate = JsonDocument.Parse(getAfterUpdateJsonStr).RootElement;
        var itemsAfterUpdate = rootAfterUpdate.ValueKind == JsonValueKind.Array 
            ? rootAfterUpdate.EnumerateArray().ToList() 
            : rootAfterUpdate.GetProperty("items").EnumerateArray().ToList();
        
        var updatedItem = itemsAfterUpdate.FirstOrDefault(x => x.GetProperty("infoId").GetString() == createdInfoId);
        updatedItem.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        updatedItem.GetProperty("infoText").GetString().Should().Be("Gaming", "Доп. информация должна была обновиться, кэш инвалидирован");
    }
}
