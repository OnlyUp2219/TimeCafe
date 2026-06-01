namespace Billing.TimeCafe.Application.CQRS.Invoices.Strategies;

public interface IInvoicePaymentStrategy
{
    PaymentMethod Method { get; }
    Task<Result> PayAsync(Invoice invoice, CancellationToken cancellationToken);
}
