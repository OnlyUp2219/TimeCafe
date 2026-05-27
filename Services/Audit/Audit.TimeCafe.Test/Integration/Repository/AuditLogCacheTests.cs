using System.Net;
using System.Text.Json;
using FluentAssertions;
using Audit.TimeCafe.Test.Integration.Helpers;
using Audit.Core;
using System.Net.Http.Json;

namespace Audit.TimeCafe.Test.Integration.Repository;

public class AuditLogCacheTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task GetById_ShouldCacheResult()
    {
        // Arrange
        var auditEvent = new AuditEvent
        {
            EventType = "TestEvent",
            Environment = new AuditEventEnvironment
            {
                UserName = "TestUser"
            }
        };

        var payload = new
        {
            EventId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            AuditEventJson = auditEvent.ToJson()
        };

        // There is no POST endpoint for Audit Logs in API directly, but we can bypass or directly insert?
        // Wait, Audit logs are consumed via MassTransit.
        // I will just use the database directly to insert one.
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Audit.TimeCafe.Infrastructure.Data.ApplicationDbContext>();
        
        var log = new Audit.TimeCafe.Domain.Entities.AuditLog(
            payload.EventId,
            payload.CreatedAt,
            auditEvent
        );
        
        context.AuditLogs.Add(log);
        await context.SaveChangesAsync();

        // 1. First GET to cache
        var initialGetResponse = await Client.GetAsync($"/audit/logs/{log.Id}");
        initialGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // 2. Change the DB directly without invalidating cache
        var logToUpdate = await context.AuditLogs.FindAsync(log.Id);
        logToUpdate!.EventType = "UpdatedEvent";
        await context.SaveChangesAsync();

        // 3. Second GET should return cached result (not updated)
        var cachedGetResponse = await Client.GetAsync($"/audit/logs/{log.Id}");
        cachedGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var cachedJsonStr = await cachedGetResponse.Content.ReadAsStringAsync();
        var cachedItem = JsonDocument.Parse(cachedJsonStr).RootElement;
        
        cachedItem.GetProperty("eventType").GetString().Should().Be("TestEvent", "The cache should return the original value even though the DB was changed manually.");
    }
}
