using Billing.TimeCafe.Application.CQRS.Invoices.Commands;
using MediatR;
using DomainPaymentMethod = Billing.TimeCafe.Domain.Enums.PaymentMethod;

namespace Billing.TimeCafe.Infrastructure.Consumers;

public sealed class VisitTimerStoppedEventConsumer : IConsumer<VisitTimerStoppedEvent>
{
    private readonly IUnitOfWork _uow;
    private readonly IPublisher _publisher;
    private readonly ILogger<VisitTimerStoppedEventConsumer> _logger;
    private readonly ISender _sender;

    public VisitTimerStoppedEventConsumer(
        IUnitOfWork uow,
        IPublisher publisher,
        ILogger<VisitTimerStoppedEventConsumer> logger,
        ISender sender)
    {
        _uow = uow;
        _publisher = publisher;
        _logger = logger;
        _sender = sender;
    }

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

            _logger.LogInformation("Успешно создан инвойс {InvoiceId} для визита {VisitId} на сумму {Amount} Br", invoice.InvoiceId, evt.VisitId, evt.Amount);

            if (evt.PayFromBalance && evt.UserId.HasValue)
            {
                var balance = await _uow.Balances.GetByIdAsync(evt.UserId.Value, cancellationToken);
                if (balance != null && balance.CurrentBalance >= evt.Amount)
                {
                    var payCommand = new PayInvoiceCommand(invoice.InvoiceId, DomainPaymentMethod.Online);
                    var payResult = await _sender.Send(payCommand, cancellationToken);
                    if (payResult.IsFailed)
                    {
                        _logger.LogWarning("Не удалось автоматически оплатить инвойс {InvoiceId} с баланса: {Error}", invoice.InvoiceId, payResult.Errors[0].Message);
                    }
                    else
                    {
                        _logger.LogInformation("Инвойс {InvoiceId} автоматически оплачен с баланса", invoice.InvoiceId);
                    }
                }
                else
                {
                    _logger.LogWarning("Недостаточно средств на балансе для автоматической оплаты инвойса {InvoiceId}", invoice.InvoiceId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке остановки таймера визита {VisitId}", evt.VisitId);
            throw;
        }
    }
}
