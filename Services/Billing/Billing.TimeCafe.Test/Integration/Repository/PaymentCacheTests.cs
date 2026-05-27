using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Billing.TimeCafe.Test.Integration;

namespace Billing.TimeCafe.Test.Integration.Repository;

public class PaymentCacheTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Cache_ShouldBeInvalidated_OnCreatePayment()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // 1. Получить платежи (Initial GET)
        var initialGetResponse = await Client.GetAsync($"/billing/payments/history/{userId}");
        initialGetResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        var initialCount = 0;
        if (initialGetResponse.IsSuccessStatusCode)
        {
            var initialJsonStr = await initialGetResponse.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(initialJsonStr).RootElement;
            var initialItems = root.ValueKind == JsonValueKind.Array 
                ? root.EnumerateArray().ToList() 
                : root.GetProperty("payments").EnumerateArray().ToList();
            initialCount = initialItems.Count;
        }

        // 2. Инициализация платежа (POST)
        var createPayload = new
        {
            userId = userId,
            amount = 100.50m,
            returnUrl = "https://example.com/success",
            description = "Test deposit"
        };

        var createResponse = await Client.PostAsJsonAsync("/billing/payments/initialize", createPayload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var createJsonStr = await createResponse.Content.ReadAsStringAsync();
        var createdPaymentId = JsonDocument.Parse(createJsonStr).RootElement.GetProperty("paymentId").GetString();

        // 3. Получить данные (GET after POST)
        var getAfterCreateResponse = await Client.GetAsync($"/billing/payments/history/{userId}");
        getAfterCreateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterCreateJsonStr = await getAfterCreateResponse.Content.ReadAsStringAsync();
        var rootAfterCreate = JsonDocument.Parse(getAfterCreateJsonStr).RootElement;
        var itemsAfterCreate = rootAfterCreate.ValueKind == JsonValueKind.Array 
            ? rootAfterCreate.EnumerateArray().ToList() 
            : rootAfterCreate.GetProperty("payments").EnumerateArray().ToList();
        
        itemsAfterCreate.Count.Should().BeGreaterThan(initialCount, "Новый платеж должен появиться в списке, кэш инвалидирован");
        var createdItem = itemsAfterCreate.FirstOrDefault(x => x.GetProperty("paymentId").GetString() == createdPaymentId);
        createdItem.ValueKind.Should().NotBe(JsonValueKind.Undefined);
    }
}
