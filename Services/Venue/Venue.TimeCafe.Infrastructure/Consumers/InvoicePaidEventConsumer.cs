namespace Venue.TimeCafe.Infrastructure.Consumers;

public class InvoicePaidEventConsumer(
    IUnitOfWork uow,
    IPublisher publisher,
    ILogger<InvoicePaidEventConsumer> logger) : IConsumer<InvoicePaidEvent>
{
    public async Task Consume(ConsumeContext<InvoicePaidEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Получено уведомление об оплате счёта {InvoiceId} для визита {VisitId}", message.InvoiceId, message.VisitId);

        var visit = await uow.Visits.GetByIdAsync(message.VisitId, context.CancellationToken);
        if (visit == null)
        {
            logger.LogError("Визит {VisitId} не найден при попытке завершить оплаченный счёт {InvoiceId}", message.VisitId, message.InvoiceId);
            return;
        }

        if (visit.Status == VisitStatus.Completed)
        {
            logger.LogInformation("Визит {VisitId} уже завершён (Completed) при получении оплаты по счёту {InvoiceId}.", message.VisitId, message.InvoiceId);
            return;
        }

        if (visit.Status == VisitStatus.Cancelled || visit.Status == VisitStatus.Rejected)
        {
            logger.LogWarning("Счёт {InvoiceId} оплачен, но визит {VisitId} имеет статус {Status}. Игнорируем.", message.InvoiceId, message.VisitId, visit.Status);
            return;
        }

        if (visit.Status == VisitStatus.Active)
        {
            logger.LogWarning("Счёт {InvoiceId} оплачен, но визит {VisitId} всё ещё в статусе Active. Фиксируем время по факту оплаты.", message.InvoiceId, message.VisitId);
            var fixateResult = visit.FixateTime(message.Amount, message.PaidAt);
            if (fixateResult.IsFailed)
            {
                throw new InvalidOperationException($"Не удалось зафиксировать время визита {message.VisitId}: {fixateResult.Errors[0].Message}");
            }
        }

        var result = visit.Complete();
        if (result.IsFailed)
        {
            throw new InvalidOperationException($"Не удалось перевести визит {message.VisitId} в статус Completed: {result.Errors[0].Message}");
        }

        await uow.Visits.UpdateAsync(visit, context.CancellationToken);
        await uow.SaveChangesAsync(context.CancellationToken);

        await publisher.Publish(new VisitChangedEvent(visit.VisitId, visit.UserId), context.CancellationToken);

        if (visit.UserId.HasValue)
        {
            await context.Publish(new BuildingBlocks.Events.VisitCompletedEvent
            {
                VisitId = visit.VisitId,
                UserId = visit.UserId.Value,
                Amount = message.Amount,
                CompletedAt = message.PaidAt
            }, context.CancellationToken);
        }

        Venue.TimeCafe.Application.Metrics.VenueMetrics.ActiveVisits.Dec();
        Venue.TimeCafe.Application.Metrics.VenueMetrics.VisitsCompleted.Inc();

        logger.LogInformation("Визит {VisitId} успешно переведен в статус Completed и столик/ресурс освобожден.", visit.VisitId);
    }
}
