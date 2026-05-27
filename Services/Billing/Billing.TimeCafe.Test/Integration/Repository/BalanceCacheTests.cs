using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Billing.TimeCafe.Test.Integration;

namespace Billing.TimeCafe.Test.Integration.Repository;

public class BalanceCacheTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Cache_ShouldBeInvalidated_OnCreateTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // 1. Получить баланс (Initial GET)
        var initialGetResponse = await Client.GetAsync($"/billing/balance/{userId}");
        var initialJsonStr = await initialGetResponse.Content.ReadAsStringAsync();
        if (initialGetResponse.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new Exception($"Internal Server Error: {initialJsonStr}");
        }
        initialGetResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        var initialBalanceAmount = 0m;
        if (initialGetResponse.IsSuccessStatusCode)
        {
            try
            {
                initialBalanceAmount = JsonDocument.Parse(initialJsonStr).RootElement.GetProperty("balance").GetProperty("amount").GetDecimal();
            }
            catch(Exception)
            {
                throw new Exception($"Failed to parse initial GET: {initialJsonStr}");
            }
        }

        // 2. Добавление транзакции (POST)
        var createPayload = new
        {
            userId = userId,
            amount = 50.0m,
            type = 1, // Deposit
            source = 2, // Manual
            comment = "Test Transaction"
        };

        var createResponse = await Client.PostAsJsonAsync("/billing/transactions", createPayload);
        var createResponseStr = await createResponse.Content.ReadAsStringAsync();
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK, $"Create response failed: {createResponseStr}");

        // 3. Получить данные (GET after POST)
        var getAfterCreateResponse = await Client.GetAsync($"/billing/balance/{userId}");
        getAfterCreateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterCreateJsonStr = await getAfterCreateResponse.Content.ReadAsStringAsync();
        var updatedBalanceAmount = JsonDocument.Parse(getAfterCreateJsonStr).RootElement.GetProperty("balance").GetProperty("amount").GetDecimal();
        
        updatedBalanceAmount.Should().Be(initialBalanceAmount + 50.0m, $"Баланс должен обновиться. Получен JSON: {getAfterCreateJsonStr}");
    }
}
