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

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedVisit = await context.Visits.FindAsync(visit.VisitId);

        updatedVisit.Should().NotBeNull();
        updatedVisit!.Status.Should().Be(VisitStatus.WaitingForPayment);
        updatedVisit.CalculatedCost.Should().NotBeNull();
        updatedVisit.CalculatedCost.Should().BeGreaterThan(0);
        updatedVisit.ExitTime.Should().NotBeNull();
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

    [Fact]
    public async Task Endpoint_FullCycleScenario_ClientFlow()
    {
        await ClearDatabaseAndCacheAsync();

        var tariff = await SeedTariffAsync("Hourly Plan", 120m, BillingType.Hourly);
        var group = await SeedResourceGroupAsync("Main Zone", 15);
        var resource = await SeedResourceAsync(group.ResourceGroupId, "Table 2", 4);

        var clientUserId = Guid.NewGuid();
        var adminUserId = Guid.Parse(TestAuthHandler.TestUserId);

        var createPayload = new
        {
            userId = clientUserId,
            tariffId = tariff.TariffId,
            resourceId = resource.ResourceId,
            plannedMinutes = 60,
            requirePositiveBalance = false,
            requireEnoughForPlanned = false,
            guestsCount = 1
        };

        using var clientRequest = new HttpRequestMessage(HttpMethod.Post, "/venue/visits")
        {
            Content = JsonContent.Create(createPayload)
        };
        clientRequest.Headers.Add("X-Test-UserId", clientUserId.ToString());
        clientRequest.Headers.Add("X-Test-UserRole", "client");

        var createResponse = await Client.SendAsync(clientRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createJson = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<Visit>(createJson, JsonOptions);
        createResult.Should().NotBeNull();
        createResult!.Status.Should().Be(VisitStatus.Pending);
        createResult.UserId.Should().Be(clientUserId);

        var visitId = createResult.VisitId;

        using var approveRequest = new HttpRequestMessage(HttpMethod.Post, $"/venue/visits/{visitId}/approve");
        approveRequest.Headers.Add("X-Test-UserId", adminUserId.ToString());
        approveRequest.Headers.Add("X-Test-UserRole", "admin");

        var approveResponse = await Client.SendAsync(approveRequest);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var approveJson = await approveResponse.Content.ReadAsStringAsync();
        var approveResult = JsonSerializer.Deserialize<Visit>(approveJson, JsonOptions);
        approveResult.Should().NotBeNull();
        approveResult!.Status.Should().Be(VisitStatus.Active);

        using var endRequest = new HttpRequestMessage(HttpMethod.Post, $"/venue/visits/{visitId}/request-end");
        endRequest.Headers.Add("X-Test-UserId", clientUserId.ToString());
        endRequest.Headers.Add("X-Test-UserRole", "client");

        var endResponse = await Client.SendAsync(endRequest);
        endResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var visitInDb = await db.Visits.FindAsync(visitId);
            visitInDb.Should().NotBeNull();
            visitInDb!.IsFinishRequested.Should().BeTrue();
        }

        using var fixateRequest = new HttpRequestMessage(HttpMethod.Post, $"/venue/visits/{visitId}/fixate-time");
        fixateRequest.Headers.Add("X-Test-UserId", adminUserId.ToString());
        fixateRequest.Headers.Add("X-Test-UserRole", "admin");

        var fixateResponse = await Client.SendAsync(fixateRequest);
        fixateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fixateJson = await fixateResponse.Content.ReadAsStringAsync();
        var fixateResult = JsonSerializer.Deserialize<FixateVisitTimeResponse>(fixateJson, JsonOptions);
        fixateResult.Should().NotBeNull();
        fixateResult!.Visit.Status.Should().Be(VisitStatus.WaitingForPayment);
        fixateResult.CalculatedCost.Should().BeGreaterThan(0);

        using (var scope = Factory.Services.CreateScope())
        {
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            await publishEndpoint.Publish(new InvoicePaidEvent
            {
                InvoiceId = Guid.NewGuid(),
                VisitId = visitId,
                UserId = clientUserId,
                Amount = fixateResult.CalculatedCost,
                PaidAt = DateTimeOffset.UtcNow
            });
        }

        Visit? updatedVisit = null;
        for (int i = 0; i < 50; i++)
        {
            using var checkScope = Factory.Services.CreateScope();
            var checkContext = checkScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            updatedVisit = await checkContext.Visits.FindAsync(visitId);
            if (updatedVisit?.Status == VisitStatus.Completed)
                break;
            await Task.Delay(100);
        }

        updatedVisit.Should().NotBeNull();
        updatedVisit!.Status.Should().Be(VisitStatus.Completed);

        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/venue/visits/{visitId}");
        getRequest.Headers.Add("X-Test-UserId", adminUserId.ToString());
        getRequest.Headers.Add("X-Test-UserRole", "admin");

        var getResponse = await Client.SendAsync(getRequest);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getJson = await getResponse.Content.ReadAsStringAsync();
        var getResult = JsonSerializer.Deserialize<Visit>(getJson, JsonOptions);
        getResult.Should().NotBeNull();
        getResult!.Status.Should().Be(VisitStatus.Completed);
    }

    [Fact]
    public async Task Endpoint_FullCycleScenario_AdminForceEndFlow()
    {
        await ClearDatabaseAndCacheAsync();

        var tariff = await SeedTariffAsync("Standard Hourly", 150m, BillingType.Hourly);
        var group = await SeedResourceGroupAsync("VIP Zone", 10);
        var resource = await SeedResourceAsync(group.ResourceGroupId, "Table VIP", 6);

        var clientUserId = Guid.NewGuid();
        var adminUserId = Guid.Parse(TestAuthHandler.TestUserId);

        var createPayload = new
        {
            userId = clientUserId,
            tariffId = tariff.TariffId,
            resourceId = resource.ResourceId,
            plannedMinutes = 120,
            requirePositiveBalance = false,
            requireEnoughForPlanned = false,
            guestsCount = 2
        };

        using var clientRequest = new HttpRequestMessage(HttpMethod.Post, "/venue/visits")
        {
            Content = JsonContent.Create(createPayload)
        };
        clientRequest.Headers.Add("X-Test-UserId", clientUserId.ToString());
        clientRequest.Headers.Add("X-Test-UserRole", "client");

        var createResponse = await Client.SendAsync(clientRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createJson = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<Visit>(createJson, JsonOptions);
        createResult.Should().NotBeNull();
        var visitId = createResult!.VisitId;

        using var approveRequest = new HttpRequestMessage(HttpMethod.Post, $"/venue/visits/{visitId}/approve");
        approveRequest.Headers.Add("X-Test-UserId", adminUserId.ToString());
        approveRequest.Headers.Add("X-Test-UserRole", "admin");

        var approveResponse = await Client.SendAsync(approveRequest);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var forceEndRequest = new HttpRequestMessage(HttpMethod.Post, $"/venue/visits/{visitId}/force-end");
        forceEndRequest.Headers.Add("X-Test-UserId", adminUserId.ToString());
        forceEndRequest.Headers.Add("X-Test-UserRole", "admin");

        var forceEndResponse = await Client.SendAsync(forceEndRequest);
        forceEndResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        decimal calculatedCost = 0m;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var visitInDb = await db.Visits.FindAsync(visitId);
            visitInDb.Should().NotBeNull();
            visitInDb!.Status.Should().Be(VisitStatus.WaitingForPayment);
            visitInDb.CalculatedCost.Should().NotBeNull();
            visitInDb.CalculatedCost.Should().BeGreaterThan(0);
            calculatedCost = visitInDb.CalculatedCost.Value;
        }

        using (var scope = Factory.Services.CreateScope())
        {
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            await publishEndpoint.Publish(new InvoicePaidEvent
            {
                InvoiceId = Guid.NewGuid(),
                VisitId = visitId,
                UserId = clientUserId,
                Amount = calculatedCost,
                PaidAt = DateTimeOffset.UtcNow
            });
        }

        Visit? updatedVisit = null;
        for (int i = 0; i < 50; i++)
        {
            using var checkScope = Factory.Services.CreateScope();
            var checkContext = checkScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            updatedVisit = await checkContext.Visits.FindAsync(visitId);
            if (updatedVisit?.Status == VisitStatus.Completed)
                break;
            await Task.Delay(100);
        }

        updatedVisit.Should().NotBeNull();
        updatedVisit!.Status.Should().Be(VisitStatus.Completed);
    }
}
