using BuildingBlocks.Events;

namespace Billing.TimeCafe.Infrastructure.Consumers;

public sealed class VisitApprovedEventConsumer(ILogger<VisitApprovedEventConsumer> logger) : IConsumer<VisitApprovedEvent>
{
    private readonly ILogger _logger = logger;

    public Task Consume(ConsumeContext<VisitApprovedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation(
            "Визит {VisitId} пользователя {UserId} одобрен менеджером {ApprovedByUserId}",
            evt.VisitId, evt.UserId, evt.ApprovedByUserId);
        return Task.CompletedTask;
    }
}
