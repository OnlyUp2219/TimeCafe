using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Venue.TimeCafe.Domain.Constants;
using Venue.TimeCafe.Test.Integration.Helpers;

namespace Venue.TimeCafe.Test.Integration.Repository;

public class VisitsCacheTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Cache_ShouldBeInvalidated_OnCreateAndUpdateEndpoints()
    {
        // Arrange
        await ClearDatabaseAndCacheAsync();
        
        var theme = await SeedThemeAsync("Visit Theme");
        var tariffId = Guid.NewGuid();
        var createTariffPayload = new
        {
            name = "Visit Tariff",
            description = "For visits",
            pricePerMinute = 10m,
            billingType = (int)BillingType.PerMinute,
            themeId = theme.ThemeId,
            isActive = true
        };
        var createTariffResponse = await Client.PostAsJsonAsync("/venue/tariffs", createTariffPayload);
        createTariffResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var tariffJsonStr = await createTariffResponse.Content.ReadAsStringAsync();
        tariffId = Guid.Parse(JsonDocument.Parse(tariffJsonStr).RootElement.GetProperty("tariffId").GetString()!);

        var userId = Guid.NewGuid(); // mock user

        // 1. Получить данные (Initial GET)
        var initialGetResponse = await Client.GetAsync("/venue/visits/active");
        initialGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var initialJsonStr = await initialGetResponse.Content.ReadAsStringAsync();
        var initialItems = JsonDocument.Parse(initialJsonStr).RootElement.EnumerateArray().ToList();
        var initialCount = initialItems.Count;

        // 2. Создание (POST)
        var createPayload = new
        {
            userId = userId,
            tariffId = tariffId,
            plannedMinutes = 60,
            requirePositiveBalance = false,
            requireEnoughForPlanned = false,
            guestsCount = 1
        };

        var createResponse = await Client.PostAsJsonAsync("/venue/visits", createPayload);
        // Note: For balance check it might fail if user doesn't exist, but Venue API just calls gRPC. 
        // We set requirePositiveBalance = false so it should bypass the HTTP/gRPC check.
        if (createResponse.StatusCode == HttpStatusCode.BadRequest)
        {
            // If it fails with bad request, it might be the user doesn't exist in Auth. 
            // We just need to check if we can bypass it or use a valid user.
            var errorContent = await createResponse.Content.ReadAsStringAsync();
            throw new Exception($"Create visit failed: {errorContent}");
        }
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createJsonStr = await createResponse.Content.ReadAsStringAsync();
        var createdVisitId = JsonDocument.Parse(createJsonStr).RootElement.GetProperty("visitId").GetString();
        createdVisitId.Should().NotBeNullOrEmpty();

        // 3. Получить данные (GET after POST)
        // Кэш GetPendingVisitsAsync должен быть инвалидирован
        var getAfterCreateResponse = await Client.GetAsync("/venue/visits/pending");
        getAfterCreateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterCreateJsonStr = await getAfterCreateResponse.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(getAfterCreateJsonStr).RootElement;
        var itemsAfterCreate = root.GetProperty("items").EnumerateArray().ToList();
        
        itemsAfterCreate.Count.Should().Be(initialCount + 1, "Новый визит должен появиться в списке, кэш инвалидирован");
        var createdItem = itemsAfterCreate.FirstOrDefault(x => x.GetProperty("visitId").GetString() == createdVisitId);
        createdItem.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        
        // 4. Обновление статуса (PUT/POST/PATCH) 
        // Для визитов обычно вызывается Complete / Cancel / Approve
        var completeResponse = await Client.PostAsync($"/venue/visits/{createdVisitId}/end", null);
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Получить данные (GET after PUT)
        var getAfterCompleteResponse = await Client.GetAsync("/venue/visits/pending");
        getAfterCompleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterCompleteJsonStr = await getAfterCompleteResponse.Content.ReadAsStringAsync();
        root = JsonDocument.Parse(getAfterCompleteJsonStr).RootElement;
        var itemsAfterComplete = root.GetProperty("items").EnumerateArray().ToList();
        
        itemsAfterComplete.Count.Should().Be(initialCount, "Завершенный визит не должен быть в списке ожидающих");
    }
}
