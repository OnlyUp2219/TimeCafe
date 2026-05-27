using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Venue.TimeCafe.Domain.Constants;
using Venue.TimeCafe.Test.Integration.Helpers;

namespace Venue.TimeCafe.Test.Integration.Repository;

public class TariffsCacheTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Cache_ShouldBeInvalidated_OnCreateAndUpdateEndpoints()
    {
        // Arrange
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Cache Test Theme");
        
        // 1. Получить данные (Initial GET)
        var initialGetResponse = await Client.GetAsync("/venue/tariffs/active");
        initialGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var initialJsonStr = await initialGetResponse.Content.ReadAsStringAsync();
        var initialItems = JsonDocument.Parse(initialJsonStr).RootElement.EnumerateArray().ToList();
        var initialCount = initialItems.Count;

        // 2. Создание (POST)
        var createPayload = new
        {
            name = "New Cached Tariff",
            description = "Tariff to test caching",
            pricePerMinute = 15m,
            billingType = (int)BillingType.PerMinute,
            themeId = theme.ThemeId,
            isActive = true
        };

        var createResponse = await Client.PostAsJsonAsync("/venue/tariffs", createPayload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createJsonStr = await createResponse.Content.ReadAsStringAsync();
        var createdTariffId = JsonDocument.Parse(createJsonStr).RootElement.GetProperty("tariffId").GetString();
        createdTariffId.Should().NotBeNullOrEmpty();

        // 3. Получить данные (GET after POST)
        // Кэш должен был инвалидироваться, и мы должны увидеть новый тариф
        var getAfterCreateResponse = await Client.GetAsync("/venue/tariffs/active");
        getAfterCreateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterCreateJsonStr = await getAfterCreateResponse.Content.ReadAsStringAsync();
        var itemsAfterCreate = JsonDocument.Parse(getAfterCreateJsonStr).RootElement.EnumerateArray().ToList();
        
        itemsAfterCreate.Count.Should().Be(initialCount + 1, "Новый тариф должен появиться в списке, кэш инвалидирован");
        var createdItem = itemsAfterCreate.FirstOrDefault(x => x.GetProperty("tariffId").GetString() == createdTariffId);
        createdItem.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        createdItem.GetProperty("name").GetString().Should().Be("New Cached Tariff");

        // 4. Обновление (PUT)
        var updatePayload = new
        {
            name = "Updated Cached Tariff",
            description = "Updated description",
            pricePerMinute = 20m,
            billingType = (int)BillingType.PerMinute,
            themeId = theme.ThemeId,
            isActive = true
        };

        var updateResponse = await Client.PutAsJsonAsync($"/venue/tariffs/{createdTariffId}", updatePayload);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Получить данные (GET after PUT)
        // Кэш должен был инвалидироваться, и мы должны увидеть обновленное имя
        var getAfterUpdateResponse = await Client.GetAsync("/venue/tariffs/active");
        getAfterUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterUpdateJsonStr = await getAfterUpdateResponse.Content.ReadAsStringAsync();
        var itemsAfterUpdate = JsonDocument.Parse(getAfterUpdateJsonStr).RootElement.EnumerateArray().ToList();
        
        itemsAfterUpdate.Count.Should().Be(initialCount + 1);
        var updatedItem = itemsAfterUpdate.FirstOrDefault(x => x.GetProperty("tariffId").GetString() == createdTariffId);
        updatedItem.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        updatedItem.GetProperty("name").GetString().Should().Be("Updated Cached Tariff", "Тариф должен был обновиться, кэш инвалидирован");
        updatedItem.GetProperty("pricePerMinute").GetDecimal().Should().Be(20m);
    }
}
