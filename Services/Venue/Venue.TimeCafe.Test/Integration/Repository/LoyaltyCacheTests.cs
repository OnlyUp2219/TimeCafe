using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Venue.TimeCafe.Domain.Constants;
using Venue.TimeCafe.Test.Integration.Helpers;

namespace Venue.TimeCafe.Test.Integration.Repository;

public class LoyaltyCacheTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Cache_ShouldBeInvalidated_OnVisitCompleted()
    {
        // Arrange
        await ClearDatabaseAndCacheAsync();
        
        var theme = await SeedThemeAsync("Loyalty Theme");
        var createTariffPayload = new
        {
            name = "Loyalty Tariff",
            description = "For testing loyalty",
            pricePerMinute = 10m,
            billingType = (int)BillingType.PerMinute,
            themeId = theme.ThemeId,
            isActive = true
        };
        var createTariffResponse = await Client.PostAsJsonAsync("/venue/tariffs", createTariffPayload);
        createTariffResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var tariffJsonStr = await createTariffResponse.Content.ReadAsStringAsync();
        var tariffId = Guid.Parse(JsonDocument.Parse(tariffJsonStr).RootElement.GetProperty("tariffId").GetString()!);

        var userId = Guid.NewGuid();

        // 1. Создаем и завершаем первый визит, чтобы создать лояльность
        var createPayload1 = new
        {
            userId = userId,
            tariffId = tariffId,
            plannedMinutes = 60,
            requirePositiveBalance = false,
            requireEnoughForPlanned = false,
            guestsCount = 1
        };

        var createVisit1Resp = await Client.PostAsJsonAsync("/venue/visits", createPayload1);
        createVisit1Resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var visit1Id = JsonDocument.Parse(await createVisit1Resp.Content.ReadAsStringAsync()).RootElement.GetProperty("visitId").GetString();

        var complete1Resp = await Client.PostAsync($"/venue/visits/{visit1Id}/end", null);
        complete1Resp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Получить лояльность (Initial GET)
        var initialGetResponse = await Client.GetAsync($"/venue/loyalty/{userId}");
        initialGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var initialJsonStr = await initialGetResponse.Content.ReadAsStringAsync();
        var initialLoyalty = JsonDocument.Parse(initialJsonStr).RootElement;
        var initialTotalSpent = initialLoyalty.GetProperty("totalSpentAmount").GetDecimal();
        var initialVisits = initialLoyalty.GetProperty("totalVisitsCount").GetInt32();

        initialVisits.Should().Be(1);

        // 3. Создаем и завершаем второй визит, чтобы обновить лояльность
        var createVisit2Resp = await Client.PostAsJsonAsync("/venue/visits", createPayload1);
        createVisit2Resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var visit2Id = JsonDocument.Parse(await createVisit2Resp.Content.ReadAsStringAsync()).RootElement.GetProperty("visitId").GetString();

        var complete2Resp = await Client.PostAsync($"/venue/visits/{visit2Id}/end", null);
        complete2Resp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Получить лояльность после обновления
        // Кэш должен быть инвалидирован, и мы должны увидеть увеличенные счетчики
        var getAfterUpdateResponse = await Client.GetAsync($"/venue/loyalty/{userId}");
        getAfterUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterUpdateJsonStr = await getAfterUpdateResponse.Content.ReadAsStringAsync();
        var updatedLoyalty = JsonDocument.Parse(getAfterUpdateJsonStr).RootElement;
        
        updatedLoyalty.GetProperty("totalVisitsCount").GetInt32().Should().Be(2, "Количество визитов должно увеличиться, кэш инвалидирован");
        updatedLoyalty.GetProperty("totalSpentAmount").GetDecimal().Should().BeGreaterThan(initialTotalSpent);
    }
}
