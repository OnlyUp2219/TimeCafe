using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using UserProfile.TimeCafe.Test.Integration.Helpers;

namespace UserProfile.TimeCafe.Test.Integration.Repository;

public class ProfileCacheTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Cache_ShouldBeInvalidated_OnCreateAndUpdateEndpoints()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // 1. Получить данные (Initial GET)
        var initialGetResponse = await Client.GetAsync($"/userprofile/profiles/{userId}");
        initialGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // 2. Создание (POST) - CreateEmptyProfile or CreateProfile
        // Let's use CreateProfile
        var createPayload = new
        {
            userId = userId,
            firstName = "Test",
            lastName = "User",
            email = "test@example.com",
            phone = "+1234567890",
            gender = 1, // Male
            birthDate = "1990-01-01"
        };

        var createResponse = await Client.PostAsJsonAsync("/userprofile/profiles", createPayload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // 3. Получить данные (GET after POST)
        var getAfterCreateResponse = await Client.GetAsync($"/userprofile/profiles/{userId}");
        getAfterCreateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterCreateJsonStr = await getAfterCreateResponse.Content.ReadAsStringAsync();
        var profileAfterCreate = JsonDocument.Parse(getAfterCreateJsonStr).RootElement;
        
        profileAfterCreate.GetProperty("firstName").GetString().Should().Be("Test");

        // 4. Обновление (PUT)
        var updatePayload = new
        {
            firstName = "Updated",
            lastName = "User",
            gender = 2, // Female
            birthDate = "1990-01-01"
        };

        var updateResponse = await Client.PutAsJsonAsync($"/userprofile/profiles/{userId}", updatePayload);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 5. Получить данные (GET after PUT)
        var getAfterUpdateResponse = await Client.GetAsync($"/userprofile/profiles/{userId}");
        getAfterUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterUpdateJsonStr = await getAfterUpdateResponse.Content.ReadAsStringAsync();
        var profileAfterUpdate = JsonDocument.Parse(getAfterUpdateJsonStr).RootElement;
        
        profileAfterUpdate.GetProperty("firstName").GetString().Should().Be("Updated", "Профиль должен обновиться, кэш инвалидирован");
    }
}
