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

        var result = visit.Complete();
        if (result.IsFailed)
        {
            logger.LogError("Не удалось перевести визит {VisitId} в статус Completed: {Error}", message.VisitId, result.Errors[0].Message);
            return;
        }

        await uow.Visits.UpdateAsync(visit, context.CancellationToken);
        await uow.SaveChangesAsync(context.CancellationToken);

        await publisher.Publish(new VisitChangedEvent(visit.VisitId, visit.UserId), context.CancellationToken);

        Venue.TimeCafe.Application.Metrics.VenueMetrics.ActiveVisits.Dec();
        Venue.TimeCafe.Application.Metrics.VenueMetrics.VisitsCompleted.Inc();

        logger.LogInformation("Визит {VisitId} успешно переведен в статус Completed и столик/ресурс освобожден.", visit.VisitId);
    }
}
