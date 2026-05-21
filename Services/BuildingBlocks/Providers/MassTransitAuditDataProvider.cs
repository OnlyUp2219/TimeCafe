using Audit.Core;
using MassTransit;

namespace BuildingBlocks.Providers;

public sealed class MassTransitAuditDataProvider(IServiceProvider serviceProvider) : AuditDataProvider
{
    public override object InsertEvent(AuditEvent auditEvent) =>
        throw new NotSupportedException("Use async overload");

    public override async Task<object> InsertEventAsync(AuditEvent auditEvent, CancellationToken ct = default)
    {
        using var scope = serviceProvider.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var eventId = Guid.NewGuid();
        await publishEndpoint.Publish<SaveAuditMessage>(new
        {
            EventId = eventId,
            CreatedAt = DateTime.UtcNow,
            AuditEventJson = auditEvent.ToJson()
        }, ct);
        return eventId;
    }
}
