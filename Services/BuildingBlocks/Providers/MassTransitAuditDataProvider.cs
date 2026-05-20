using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Providers;

public class MassTransitAuditDataProvider(IServiceProvider serviceProvider) : AuditDataProvider
{
    public override object InsertEvent(AuditEvent auditEvent)
    {
        return InsertEventAsync(auditEvent, CancellationToken.None).GetAwaiter().GetResult();
    }

    public override async Task<object> InsertEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var eventId = Guid.NewGuid();

        await publishEndpoint.Publish<SaveAuditMessage>(new
        {
            EventId = eventId,
            CreatedAt = auditEvent.StartDate == default ? DateTime.UtcNow : auditEvent.StartDate,
            AuditEventJson = auditEvent.ToJson()
        }, cancellationToken);

        return eventId;
    }
}
