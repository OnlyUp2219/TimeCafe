using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using UserProfile.TimeCafe.Test.Integration.Helpers;
using BuildingBlocks.Events;
using MassTransit;

namespace UserProfile.TimeCafe.Test.Integration.Consumers;

public class VisitCompletedIntegrationTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task VisitCompletedEvent_Should_IncrementVisitCount_And_UpdateLoyaltyDiscount_ThroughEndpoints()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // 1. Создаем профиль пользователя через POST-запрос
        var createPayload = new
        {
            userId = userId,
            firstName = "Loyalty",
            lastName = "Tester",
            email = "loyalty@example.com",
            phone = "+79991234567",
            gender = 1, // Male
            birthDate = "1995-05-15"
        };
        
        var createResponse = await Client.PostAsJsonAsync("/userprofile/profiles", createPayload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Проверяем, что изначально визитов 0
        var initialGetResponse = await Client.GetAsync($"/userprofile/profiles/{userId}");
        initialGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var initialJson = await initialGetResponse.Content.ReadAsStringAsync();
        var initialProfile = JsonDocument.Parse(initialJson).RootElement;
        initialProfile.GetProperty("visitCount").GetInt32().Should().Be(0);

        var busControl = Factory.Services.GetRequiredService<IBusControl>();
        await Task.Delay(1000);
        await busControl.Publish(new VisitCompletedEvent
        {
            VisitId = Guid.NewGuid(),
            UserId = userId,
            Amount = 100.0m,
            CompletedAt = DateTimeOffset.UtcNow
        });

        // 3. Ждем обработки сообщения consumer-ом, опрашивая GET-эндпоинт профиля
        var timeout = TimeSpan.FromSeconds(15);
        var delay = TimeSpan.FromMilliseconds(500);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var visitCount = 0;

        while (stopwatch.Elapsed < timeout)
        {
            using (var scope = Factory.Services.CreateScope())
            {
                var cache = scope.ServiceProvider.GetRequiredService<HybridCache>();
                await cache.RemoveAsync(CacheKeys.Profile_ById(userId));
            }

            var getResponse = await Client.GetAsync($"/userprofile/profiles/{userId}");
            if (getResponse.StatusCode == HttpStatusCode.OK)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var profile = JsonDocument.Parse(json).RootElement;
                visitCount = profile.GetProperty("visitCount").GetInt32();
                if (visitCount > 0)
                {
                    break;
                }
            }
            await Task.Delay(delay);
        }

        // Assert
        visitCount.Should().Be(1, "Счетчик визитов должен увеличиться на 1 после обработки события VisitCompletedEvent");
    }

    [Fact]
    public async Task VisitCompletedEvent_Should_UpdateLoyaltyDiscount_When_TierReached_ThroughEndpoints()
    {
        var userId = Guid.NewGuid();
        await SeedProfileWithVisitsAsync(userId, 4, null);

        var busControl = Factory.Services.GetRequiredService<IBusControl>();
        await Task.Delay(1000);
        await busControl.Publish(new VisitCompletedEvent
        {
            VisitId = Guid.NewGuid(),
            UserId = userId,
            Amount = 150.0m,
            CompletedAt = DateTimeOffset.UtcNow
        });

        var timeout = TimeSpan.FromSeconds(15);
        var delay = TimeSpan.FromMilliseconds(500);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var visitCount = 0;
        decimal? discount = null;

        while (stopwatch.Elapsed < timeout)
        {
            using (var scope = Factory.Services.CreateScope())
            {
                var cache = scope.ServiceProvider.GetRequiredService<HybridCache>();
                await cache.RemoveAsync(CacheKeys.Profile_ById(userId));
            }

            var getResponse = await Client.GetAsync($"/userprofile/profiles/{userId}");
            if (getResponse.StatusCode == HttpStatusCode.OK)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var profile = JsonDocument.Parse(json).RootElement;
                visitCount = profile.GetProperty("visitCount").GetInt32();
                if (profile.TryGetProperty("personalDiscountPercent", out var discProp) && discProp.ValueKind != JsonValueKind.Null)
                {
                    discount = discProp.GetDecimal();
                }
                if (visitCount > 4 && discount.HasValue)
                {
                    break;
                }
            }
            await Task.Delay(delay);
        }

        visitCount.Should().Be(5);
        discount.Should().Be(5m);
    }

    private async Task SeedProfileWithVisitsAsync(Guid userId, int visitCount, decimal? discount)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var profile = new UserProfile.TimeCafe.Domain.Models.Profile
        {
            UserId = userId,
            FirstName = "Loyalty",
            LastName = "Tester",
            Gender = UserProfile.TimeCafe.Domain.Enums.Gender.Male,
            ProfileStatus = UserProfile.TimeCafe.Domain.Enums.ProfileStatus.Completed,
            CreatedAt = DateTimeOffset.UtcNow,
            VisitCount = visitCount,
            PersonalDiscountPercent = discount
        };
        context.Profiles.Add(profile);
        await context.SaveChangesAsync();
    }
}
