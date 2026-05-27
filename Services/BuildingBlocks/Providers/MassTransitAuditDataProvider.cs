using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Audit.Core;
using MassTransit;

namespace BuildingBlocks.Providers;

public sealed class MassTransitAuditDataProvider(IBus bus, ILogger<MassTransitAuditDataProvider> logger) : AuditDataProvider
{
    public override object InsertEvent(AuditEvent auditEvent) =>
        throw new NotSupportedException("Use async overload");

    public override async Task<object> InsertEventAsync(AuditEvent auditEvent, CancellationToken ct = default)
    {
        var eventId = Guid.NewGuid();
        
        logger.LogInformation("MassTransitAuditDataProvider publishing SaveAuditMessage for EventType {EventType} via IBus", auditEvent.EventType);
        
        await bus.Publish<SaveAuditMessage>(new
        {
            EventId = eventId,
            CreatedAt = DateTime.UtcNow,
            AuditEventJson = auditEvent.ToJson()
        }, ct);
        return eventId;
    }
}
