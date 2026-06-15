namespace Billing.TimeCafe.Application.CQRS.Invoices.Commands;
using Billing.TimeCafe.Application.Services;

public record PayInvoiceCommand(Guid InvoiceId, PaymentMethod Method) : ICommand;

public class PayInvoiceCommandValidator : AbstractValidator<PayInvoiceCommand>
{
    public PayInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId).ValidGuidEntityId("Инвойс не найден");
        RuleFor(x => x.Method).IsInEnum().WithMessage("Некорректный метод оплаты");
    }
}

public class PayInvoiceCommandHandler(
    IUnitOfWork uow,
    IPublishEndpoint publishEndpoint,
    IPublisher publisher,
    IEnumerable<IInvoicePaymentStrategy> strategies,
    IFiscalService fiscalService) : ICommandHandler<PayInvoiceCommand>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IPublisher _publisher = publisher;
    private readonly IEnumerable<IInvoicePaymentStrategy> _strategies = strategies;
    private readonly IFiscalService _fiscalService = fiscalService;

    public async Task<Result> Handle(PayInvoiceCommand request, CancellationToken cancellationToken = default)
    {
        var invoice = await _uow.Invoices.GetByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice == null)
            return Result.Fail(new InvoiceNotFoundError());

        var strategy = _strategies.FirstOrDefault(s => s.Method == request.Method);
        if (strategy == null)
            return Result.Fail(new Error("Неподдерживаемый метод оплаты").WithMetadata("ErrorCode", "400"));

        var payResult = await strategy.PayAsync(invoice, cancellationToken);
        if (payResult.IsFailed)
            return payResult;

        var fiscalResult = await _fiscalService.RegisterReceiptAsync(invoice, cancellationToken);
        if (fiscalResult.IsSuccess)
        {
            invoice.UpdateFiscalData(fiscalResult.Value.ReceiptNumber, fiscalResult.Value.Url);
        }

        await _uow.Invoices.UpdateAsync(invoice, cancellationToken);
        await _publisher.Publish(new InvoiceChangedEvent(invoice.InvoiceId, invoice.UserId, invoice.VisitId), cancellationToken);

        await _publishEndpoint.Publish(new InvoicePaidEvent
        {
            InvoiceId = invoice.InvoiceId,
            VisitId = invoice.VisitId,
            UserId = invoice.UserId,
            Amount = invoice.TotalAmount,
            PaidAt = invoice.PaidAt ?? DateTimeOffset.UtcNow
        }, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        if (request.Method == PaymentMethod.Online && invoice.UserId.HasValue)
        {
            await _publisher.Publish(new BalanceChangedEvent(invoice.UserId.Value), cancellationToken);
        }

        return Result.Ok();
    }
}
