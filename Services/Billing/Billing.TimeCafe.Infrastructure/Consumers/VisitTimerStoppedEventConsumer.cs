namespace Billing.TimeCafe.Infrastructure.Consumers;

public sealed class VisitTimerStoppedEventConsumer(
    IUnitOfWork uow,
    IPublisher publisher,
    ILogger<VisitTimerStoppedEventConsumer> logger) : IConsumer<VisitTimerStoppedEvent>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;
    private readonly ILogger<VisitTimerStoppedEventConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<VisitTimerStoppedEvent> context)
    {
        var evt = context.Message;
        var cancellationToken = context.CancellationToken;

        try
        {
            var existing = await _uow.Invoices.GetByVisitIdAsync(evt.VisitId, cancellationToken);
            if (existing is not null)
            {
                _logger.LogWarning("Инвойс для визита {VisitId} уже существует (идемпотентность)", evt.VisitId);
                return;
            }

            var invoiceResult = Invoice.Create(evt.UserId, evt.VisitId, evt.Amount);
            if (invoiceResult.IsFailed)
            {
                _logger.LogError("Не удалось создать инвойс для визита {VisitId}: {Error}", evt.VisitId, invoiceResult.Errors[0].Message);
                return;
            }

            var invoice = invoiceResult.Value;
            invoice.CreatedAt = evt.StoppedAt;

            await _uow.Invoices.CreateAsync(invoice, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(new InvoiceChangedEvent(invoice.InvoiceId, invoice.UserId, invoice.VisitId), cancellationToken);

            _logger.LogInformation("Успешно создан инвойс {InvoiceId} для визита {VisitId} на сумму {Amount}₽", invoice.InvoiceId, evt.VisitId, evt.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке остановки таймера визита {VisitId}", evt.VisitId);
            throw;
        }
    }
}
