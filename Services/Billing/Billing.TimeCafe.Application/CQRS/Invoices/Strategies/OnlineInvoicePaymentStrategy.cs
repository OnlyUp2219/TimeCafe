namespace Billing.TimeCafe.Application.CQRS.Invoices.Strategies;

public class OnlineInvoicePaymentStrategy(IUnitOfWork uow) : IInvoicePaymentStrategy
{
    private readonly IUnitOfWork _uow = uow;

    public PaymentMethod Method => PaymentMethod.Online;

    public async Task<Result> PayAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        if (!invoice.UserId.HasValue)
            return Result.Fail(new Error("Анонимный визит нельзя оплатить с баланса").WithMetadata("ErrorCode", "400"));

        var payResult = invoice.Pay(PaymentMethod.Online);
        if (payResult.IsFailed)
            return payResult;

        var balance = await _uow.Balances.GetByIdAsync(invoice.UserId.Value, cancellationToken);
        if (balance == null)
        {
            balance = Balance.Create(invoice.UserId.Value);
            await _uow.Balances.CreateAsync(balance, cancellationToken);
        }

        var withdrawResult = balance.Withdraw(invoice.TotalAmount);
        if (withdrawResult.IsFailed)
            return withdrawResult;

        var transaction = Transaction.CreateWithdrawal(
            invoice.UserId.Value,
            invoice.TotalAmount,
            TransactionSource.Visit,
            invoice.VisitId,
            $"Оплата визита #{invoice.VisitId} с баланса"
        );
        transaction.BalanceAfter = balance.CurrentBalance;

        await _uow.Balances.UpdateAsync(balance, cancellationToken);
        await _uow.Transactions.CreateAsync(transaction, cancellationToken);

        return Result.Ok();
    }
}
