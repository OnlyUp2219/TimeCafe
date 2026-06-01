namespace Billing.TimeCafe.Application.CQRS.Invoices.Commands;

public record InitializeStripeInvoicePaymentCommand(Guid InvoiceId, string SuccessUrl, string CancelUrl) : ICommand<InitializeStripeInvoicePaymentResponse>;

public class InitializeStripeInvoicePaymentCommandValidator : AbstractValidator<InitializeStripeInvoicePaymentCommand>
{
    public InitializeStripeInvoicePaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId).ValidGuidEntityId("Инвойс не найден");
        RuleFor(x => x.SuccessUrl).NotEmpty().WithMessage("Укажите URL для перенаправления при успешной оплате");
        RuleFor(x => x.CancelUrl).NotEmpty().WithMessage("Укажите URL для перенаправления при отмене оплаты");
    }
}

public record InitializeStripeInvoicePaymentResponse(string SessionId, string CheckoutUrl);

public class InitializeStripeInvoicePaymentCommandHandler(
    IUnitOfWork uow,
    IStripePaymentClient stripePaymentClient,
    IPublisher publisher) : ICommandHandler<InitializeStripeInvoicePaymentCommand, InitializeStripeInvoicePaymentResponse>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IStripePaymentClient _stripePaymentClient = stripePaymentClient;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<InitializeStripeInvoicePaymentResponse>> Handle(InitializeStripeInvoicePaymentCommand request, CancellationToken cancellationToken = default)
    {
        var invoice = await _uow.Invoices.GetByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice == null)
            return Result.Fail<InitializeStripeInvoicePaymentResponse>(new InvoiceNotFoundError());

        if (invoice.Status == InvoiceStatus.Paid)
            return Result.Fail<InitializeStripeInvoicePaymentResponse>(new InvoiceAlreadyPaidError());

        var paymentResult = Payment.Create(
            invoice.UserId ?? Guid.Empty,
            invoice.TotalAmount,
            PaymentMethod.Online
        );

        if (paymentResult.IsFailed)
            return Result.Fail<InitializeStripeInvoicePaymentResponse>(paymentResult.Errors);

        var payment = paymentResult.Value;
        
        var stripeRequest = new StripeCreateCheckoutSessionRequest(
            payment.PaymentId,
            invoice.UserId ?? Guid.Empty,
            invoice.TotalAmount,
            "rub",
            $"Оплата счёта #{invoice.InvoiceId}",
            request.SuccessUrl,
            request.CancelUrl,
            invoice.InvoiceId
        );

        var stripeSession = await _stripePaymentClient.CreateCheckoutSessionAsync(stripeRequest, cancellationToken);
        if (!stripeSession.Success || string.IsNullOrWhiteSpace(stripeSession.SessionId) || string.IsNullOrWhiteSpace(stripeSession.CheckoutUrl))
        {
            return Result.Fail<InitializeStripeInvoicePaymentResponse>(new Error(stripeSession.Error ?? "Ошибка при создании Stripe сессии").WithMetadata("ErrorCode", "500"));
        }

        payment.ExternalPaymentId = stripeSession.SessionId;
        invoice.StripeSessionId = stripeSession.SessionId;

        await _uow.Payments.CreateAsync(payment, cancellationToken);
        await _uow.Invoices.UpdateAsync(invoice, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new InvoiceChangedEvent(invoice.InvoiceId, invoice.UserId, invoice.VisitId), cancellationToken);

        return Result.Ok(new InitializeStripeInvoicePaymentResponse(stripeSession.SessionId, stripeSession.CheckoutUrl));
    }
}
