using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Venue.TimeCafe.Application.CQRS.Visits.Commands;
using Venue.TimeCafe.Domain.Enums;
using Venue.TimeCafe.Test.Integration.Helpers;
using Xunit;

namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class Slice21VisitScenariosTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_WalkInVisit_Should_CreateActiveVisit_ForAnonymousGuest()
    {
        await ClearDatabaseAndCacheAsync();

        var tariff = await SeedTariffAsync("Standard Plan", 10m);
        var group = await SeedResourceGroupAsync("Main Zone", 15);
        var resource = await SeedResourceAsync(group.ResourceGroupId, "Table 1", 4);

        var request = new WalkInVisitCommand(
            TariffId: tariff.TariffId,
            ResourceId: resource.ResourceId,
            UserId: null, // Анонимный гость
            GuestsCount: 2
        );

        var response = await Client.PostAsJsonAsync("/venue/visits/walk-in", request);
        var jsonString = await response.Content.ReadAsStringAsync();

        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = JsonSerializer.Deserialize<WalkInVisitResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            result.Should().NotBeNull();
            result!.TariffId.Should().Be(tariff.TariffId);
            result.ResourceId.Should().Be(resource.ResourceId);
            result.UserId.Should().BeNull();
            result.Status.Should().Be(VisitStatus.Active);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_WalkInVisit_Should_CreateActiveVisit_ForAnonymousGuest] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_RequestEndAndFixateTime_Should_StopTimerAndTransitionStatus()
    {
        await ClearDatabaseAndCacheAsync();

        var tariff = await SeedTariffAsync("Standard Plan", 12m);
        var group = await SeedResourceGroupAsync("Main Zone", 15);
        var resource = await SeedResourceAsync(group.ResourceGroupId, "Table 2", 4);

        // Сидируем активный визит
        var visit = await SeedVisitAsync(userId: null, tariffId: tariff.TariffId, isActive: true, status: VisitStatus.Active, resourceId: resource.ResourceId);

        // 1. Запрашиваем завершение
        var requestEndResponse = await Client.PostAsync($"/venue/visits/{visit.VisitId}/request-end", null);
        requestEndResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Фиксируем время на кассе
        var fixateResponse = await Client.PostAsync($"/venue/visits/{visit.VisitId}/fixate-time", null);
        var fixateJson = await fixateResponse.Content.ReadAsStringAsync();

        try
        {
            fixateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = JsonSerializer.Deserialize<FixateVisitTimeResponse>(fixateJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            result.Should().NotBeNull();
            result!.Visit.Status.Should().Be(VisitStatus.WaitingForPayment);
            result.CalculatedCost.Should().BeGreaterThan(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_RequestEndAndFixateTime_Should_StopTimerAndTransitionStatus] Response: {fixateJson}");
            throw;
        }
    }
}
