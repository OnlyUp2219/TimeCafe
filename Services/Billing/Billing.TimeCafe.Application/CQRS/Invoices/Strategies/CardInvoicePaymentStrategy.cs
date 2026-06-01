namespace Billing.TimeCafe.Application.CQRS.Invoices.Strategies;

public class CardInvoicePaymentStrategy(IUnitOfWork uow) : IInvoicePaymentStrategy
{
    private readonly IUnitOfWork _uow = uow;

    public PaymentMethod Method => PaymentMethod.Card;

    public async Task<Result> PayAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        var payResult = invoice.Pay(PaymentMethod.Card);
        if (payResult.IsFailed)
            return payResult;

        var paymentResult = Payment.Create(
            invoice.UserId ?? Guid.Empty,
            invoice.TotalAmount,
            PaymentMethod.Card
        );

        if (paymentResult.IsFailed)
            return paymentResult.ToResult();

        var payment = paymentResult.Value;
        payment.MarkAsSucceeded(Guid.Empty);
        await _uow.Payments.CreateAsync(payment, cancellationToken);

        return Result.Ok();
    }
}
