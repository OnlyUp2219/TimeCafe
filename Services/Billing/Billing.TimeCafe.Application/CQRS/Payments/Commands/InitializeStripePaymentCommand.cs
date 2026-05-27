namespace Billing.TimeCafe.Application.CQRS.Payments.Commands;

public sealed record InitializeStripePaymentCommand(
    Guid UserId,
    decimal Amount,
    string? ReturnUrl,
    string? Description) : ICommand<InitializeStripePaymentResponse>;

public sealed record InitializeStripePaymentResponse(
    Guid PaymentId,
    string? ExternalPaymentId,
    string? ClientSecret,
    string? PublishableKey);

public sealed class InitializeStripePaymentCommandValidator : AbstractValidator<InitializeStripePaymentCommand>
{
    public InitializeStripePaymentCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");
        RuleFor(x => x.Amount).ValidPaymentAmount();
        RuleFor(x => x.ReturnUrl).ValidUrl("ReturnUrl");
    }
}

public sealed class InitializeStripePaymentCommandHandler(
    IUnitOfWork uow,
    IStripePaymentClient stripeClient,
    IOptionsSnapshot<StripeOptions> options,
    IPublisher publisher) : ICommandHandler<InitializeStripePaymentCommand, InitializeStripePaymentResponse>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IStripePaymentClient _stripeClient = stripeClient;
    private readonly IOptionsSnapshot<StripeOptions> _options = options;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<InitializeStripePaymentResponse>> Handle(InitializeStripePaymentCommand request, CancellationToken cancellationToken = default)
    {
        var settings = _options.Value;
        var currency = settings.DefaultCurrency;

        var returnUrl = string.IsNullOrWhiteSpace(request.ReturnUrl) ? settings.DefaultReturnUrl : request.ReturnUrl;

        if (string.IsNullOrWhiteSpace(returnUrl))
            return Result.Fail(new StripeCheckoutError("Платежный провайдер не настроен (ReturnUrl missing)"));

        var description = string.IsNullOrWhiteSpace(request.Description)
            ? $"Пополнение баланса пользователя {request.UserId}"
            : request.Description;

        var paymentResult = Payment.Create(request.UserId, request.Amount);
        if (paymentResult.IsFailed)
        {
            return paymentResult.ToResult<InitializeStripePaymentResponse>();
        }

        var payment = paymentResult.Value;
        await _uow.Payments.CreateAsync(payment, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var createRequest = new StripeCreatePaymentRequest(
            payment.PaymentId,
            payment.UserId,
            payment.Amount,
            currency,
            description,
            returnUrl);

        var providerResponse = await _stripeClient.CreatePaymentAsync(createRequest, cancellationToken);

        if (!providerResponse.Success)
        {
            payment.MarkAsFailed(providerResponse.Error ?? "Stripe returns error");
            await _uow.Payments.UpdateAsync(payment, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            return Result.Fail(new StripeCheckoutError(providerResponse.Error ?? "Stripe returns error"));
        }

        payment.ExternalPaymentId = providerResponse.ExternalPaymentId;
        await _uow.Payments.UpdateAsync(payment, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new PaymentChangedEvent(payment.PaymentId, payment.UserId), cancellationToken);

        return Result.Ok(new InitializeStripePaymentResponse(
            payment.PaymentId, 
            payment.ExternalPaymentId, 
            providerResponse.ClientSecret, 
            providerResponse.PublishableKey));
    }
}
