namespace Audit.TimeCafe.Infrastructure.Consumers;

public class SaveAuditConsumer(IUnitOfWork unitOfWork, ILogger<SaveAuditConsumer> logger) : IConsumer<SaveAuditMessage>
{
    public async Task Consume(ConsumeContext<SaveAuditMessage> context)
    {
        try
        {
            var auditEvent = JsonSerializer.Deserialize<AuditEvent>(context.Message.AuditEventJson)
                ?? throw new InvalidOperationException("Failed to deserialize AuditEvent from SaveAuditMessage");

            var log = new AuditLog(
                context.Message.EventId,
                context.Message.CreatedAt,
                auditEvent
            );

            await unitOfWork.AuditLogs.CreateAsync(log);
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving audit log for event {EventId}", context.Message.EventId);
            throw;
        }
    }
}
