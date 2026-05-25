using BuildingBlocks.Events;

namespace UserProfile.TimeCafe.Infrastructure.Consumers;

public class VisitApprovedEventConsumer(ILogger<VisitApprovedEventConsumer> logger) : IConsumer<VisitApprovedEvent>
{
    public Task Consume(ConsumeContext<VisitApprovedEvent> context)
    {
        var evt = context.Message;
        logger.LogInformation(
            "Визит {VisitId} пользователя {UserId} одобрен менеджером {ApprovedByUserId}",
            evt.VisitId, evt.UserId, evt.ApprovedByUserId);
        return Task.CompletedTask;
    }
}
