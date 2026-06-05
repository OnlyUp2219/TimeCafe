namespace Billing.TimeCafe.Application.CQRS.Invoices.Strategies;

public class CashInvoicePaymentStrategy(IUnitOfWork uow) : IInvoicePaymentStrategy
{
    private readonly IUnitOfWork _uow = uow;

    public PaymentMethod Method => PaymentMethod.Cash;

    public async Task<Result> PayAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        var payResult = invoice.Pay(PaymentMethod.Cash);
        if (payResult.IsFailed)
            return payResult;

        var paymentResult = Payment.Create(
            invoice.UserId,
            invoice.TotalAmount,
            PaymentMethod.Cash
        );

        if (paymentResult.IsFailed)
            return paymentResult.ToResult();

        var payment = paymentResult.Value;
        payment.MarkAsSucceeded(null);
        await _uow.Payments.CreateAsync(payment, cancellationToken);

        return Result.Ok();
    }
}
