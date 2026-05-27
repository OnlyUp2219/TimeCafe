namespace Audit.TimeCafe.Infrastructure.Consumers;

public class SaveAuditConsumer(IUnitOfWork unitOfWork, ILogger<SaveAuditConsumer> logger) : IConsumer<SaveAuditMessage>
{
    public async Task Consume(ConsumeContext<SaveAuditMessage> context)
    {
        try
        {
            var auditEvent = AuditEvent.FromJson(context.Message.AuditEventJson)
                ?? throw new InvalidOperationException("Failed to deserialize AuditEvent from SaveAuditMessage");

            logger.LogInformation("Attempting to save AuditLog for EventId {EventId}, EventType: {EventType}", context.Message.EventId, auditEvent.EventType);
            var log = new AuditLog(
                context.Message.EventId,
                context.Message.CreatedAt,
                auditEvent
            );

            await unitOfWork.AuditLogs.CreateAsync(log);
            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Successfully saved AuditLog for EventId {EventId}", context.Message.EventId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving audit log for event {EventId}", context.Message.EventId);
            throw;
        }
    }
}
