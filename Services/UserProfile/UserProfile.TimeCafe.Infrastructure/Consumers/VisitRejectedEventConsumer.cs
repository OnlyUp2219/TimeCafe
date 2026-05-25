using BuildingBlocks.Events;

namespace UserProfile.TimeCafe.Infrastructure.Consumers;

public class VisitRejectedEventConsumer(ILogger<VisitRejectedEventConsumer> logger) : IConsumer<VisitRejectedEvent>
{
    public Task Consume(ConsumeContext<VisitRejectedEvent> context)
    {
        var evt = context.Message;
        logger.LogInformation(
            "Визит {VisitId} пользователя {UserId} отклонён. Причина: {Reason}",
            evt.VisitId, evt.UserId, evt.Reason);
        return Task.CompletedTask;
    }
}
