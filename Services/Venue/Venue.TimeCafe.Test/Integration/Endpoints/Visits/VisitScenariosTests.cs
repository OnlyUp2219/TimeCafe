namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class VisitScenariosTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

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
            UserId: null,
            GuestsCount: 2
        );

        var response = await Client.PostAsJsonAsync("/venue/visits/walk-in", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var jsonString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<WalkInVisitResponse>(jsonString, JsonOptions);

        result.Should().NotBeNull();
        result!.TariffId.Should().Be(tariff.TariffId);
        result.ResourceId.Should().Be(resource.ResourceId);
        result.UserId.Should().BeNull();
        result.Status.Should().Be(VisitStatus.Active);
    }

    [Fact]
    public async Task Endpoint_RequestEnd_Should_ReturnForbidden_When_VisitBelongsToAnotherUser()
    {
        await ClearDatabaseAndCacheAsync();

        var tariff = await SeedTariffAsync("Standard Plan", 12m);
        var anotherUserId = Guid.NewGuid();
        var visit = await SeedVisitAsync(userId: anotherUserId, tariffId: tariff.TariffId, isActive: true, status: VisitStatus.Active);

        var response = await Client.PostAsync($"/venue/visits/{visit.VisitId}/request-end", null);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Endpoint_RequestEnd_Should_Succeed_When_VisitBelongsToCurrentUser()
    {
        await ClearDatabaseAndCacheAsync();

        var tariff = await SeedTariffAsync("Standard Plan", 12m);
        var currentUserId = Guid.Parse(TestAuthHandler.TestUserId);
        var visit = await SeedVisitAsync(userId: currentUserId, tariffId: tariff.TariffId, isActive: true, status: VisitStatus.Active);

        var response = await Client.PostAsync($"/venue/visits/{visit.VisitId}/request-end", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Endpoint_ForceEnd_Should_Succeed_ForAdmin_When_VisitBelongsToAnotherUser()
    {
        await ClearDatabaseAndCacheAsync();

        var tariff = await SeedTariffAsync("Standard Plan", 12m);
        var anotherUserId = Guid.NewGuid();
        var visit = await SeedVisitAsync(userId: anotherUserId, tariffId: tariff.TariffId, isActive: true, status: VisitStatus.Active);

        var response = await Client.PostAsync($"/venue/visits/{visit.VisitId}/force-end", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Endpoint_FullCycle_Should_TransitionToCompleted_OnPayment()
    {
        await ClearDatabaseAndCacheAsync();

        var tariff = await SeedTariffAsync("Hourly Plan", 120m, BillingType.Hourly);
        var group = await SeedResourceGroupAsync("Main Zone", 15);
        var resource = await SeedResourceAsync(group.ResourceGroupId, "Table 2", 4);

        var visit = await SeedVisitAsync(userId: Guid.Parse(TestAuthHandler.TestUserId), tariffId: tariff.TariffId, isActive: true, status: VisitStatus.Active, resourceId: resource.ResourceId);

        var requestEndResponse = await Client.PostAsync($"/venue/visits/{visit.VisitId}/request-end", null);
        requestEndResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fixateResponse = await Client.PostAsync($"/venue/visits/{visit.VisitId}/fixate-time", null);
        fixateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fixateJson = await fixateResponse.Content.ReadAsStringAsync();
        var fixateResult = JsonSerializer.Deserialize<FixateVisitTimeResponse>(fixateJson, JsonOptions);

        fixateResult.Should().NotBeNull();
        fixateResult!.Visit.Status.Should().Be(VisitStatus.WaitingForPayment);

        using (var scope = Factory.Services.CreateScope())
        {
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            await publishEndpoint.Publish(new InvoicePaidEvent
            {
                InvoiceId = Guid.NewGuid(),
                VisitId = visit.VisitId,
                UserId = visit.UserId,
                Amount = fixateResult.CalculatedCost,
                PaidAt = DateTimeOffset.UtcNow
            });
        }

        Visit? updatedVisit = null;
        for (int i = 0; i < 50; i++)
        {
            using var checkScope = Factory.Services.CreateScope();
            var checkContext = checkScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            updatedVisit = await checkContext.Visits.FindAsync(visit.VisitId);
            if (updatedVisit?.Status == VisitStatus.Completed)
                break;
            await Task.Delay(100);
        }

        updatedVisit.Should().NotBeNull();
        updatedVisit!.Status.Should().Be(VisitStatus.Completed);
    }
}
