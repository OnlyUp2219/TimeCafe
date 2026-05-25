using BuildingBlocks.Events;

namespace Billing.TimeCafe.Infrastructure.Consumers;

public sealed class VisitRejectedEventConsumer(ILogger<VisitRejectedEventConsumer> logger) : IConsumer<VisitRejectedEvent>
{
    private readonly ILogger _logger = logger;

    public Task Consume(ConsumeContext<VisitRejectedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation(
            "Визит {VisitId} пользователя {UserId} отклонён. Причина: {Reason}",
            evt.VisitId, evt.UserId, evt.Reason);
        return Task.CompletedTask;
    }
}
