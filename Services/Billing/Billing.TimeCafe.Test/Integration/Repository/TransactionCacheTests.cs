using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Billing.TimeCafe.Test.Integration;

namespace Billing.TimeCafe.Test.Integration.Repository;

public class TransactionCacheTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Cache_ShouldBeInvalidated_OnCreateTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // 1. Получить транзакции (Initial GET)
        var initialGetResponse = await Client.GetAsync($"/billing/transactions/history/{userId}");
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

        // 2. Добавление транзакции (POST)
        var createPayload = new
        {
            userId = userId,
            amount = 150.00m,
            type = 1, // Deposit
            source = 2, // Manual
            comment = "Test deposit"
        };

        var createResponse = await Client.PostAsJsonAsync("/billing/transactions", createPayload);
        var createResponseStr = await createResponse.Content.ReadAsStringAsync();
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK, $"Create response failed: {createResponseStr}");
        
        var createJsonStr = await createResponse.Content.ReadAsStringAsync();
        var createdTransactionId = JsonDocument.Parse(createJsonStr).RootElement.GetProperty("transaction").GetProperty("id").GetString();

        // 3. Получить данные (GET after POST)
        var getAfterCreateResponse = await Client.GetAsync($"/billing/transactions/history/{userId}");
        getAfterCreateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterCreateJsonStr = await getAfterCreateResponse.Content.ReadAsStringAsync();
        var rootAfterCreate = JsonDocument.Parse(getAfterCreateJsonStr).RootElement;
        var itemsAfterCreate = rootAfterCreate.ValueKind == JsonValueKind.Array 
            ? rootAfterCreate.EnumerateArray().ToList() 
            : rootAfterCreate.GetProperty("items").EnumerateArray().ToList();
        
        itemsAfterCreate.Count.Should().BeGreaterThan(initialCount, "Новая транзакция должна появиться в списке, кэш инвалидирован");
        var createdItem = itemsAfterCreate.FirstOrDefault(x => x.GetProperty("transactionId").GetString() == createdTransactionId);
        createdItem.ValueKind.Should().NotBe(JsonValueKind.Undefined);
    }
}
