namespace Billing.TimeCafe.Application.CQRS.Balances.Commands;

public sealed record AdjustBalanceCommand(
    Guid UserId,
    decimal Amount,
    TransactionType Type,
    TransactionSource Source,
    Guid? SourceId = null,
    string? Comment = null) : ICommand<AdjustBalanceResponse>;

public sealed record AdjustBalanceResponse(
    Guid UserId,
    decimal CurrentBalance,
    Guid TransactionId,
    decimal TransactionAmount,
    string TransactionType);

public sealed class AdjustBalanceCommandValidator : AbstractValidator<AdjustBalanceCommand>
{
    public AdjustBalanceCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");

        RuleFor(x => x.SourceId).ValidOptionalGuidEntityId("Некорректный SourceId")
            .When(x => x.SourceId.HasValue);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Сумма должна быть больше нуля");

        RuleFor(x => x.Comment).ValidComment();
    }
}

public sealed class AdjustBalanceCommandHandler(
    IUnitOfWork uow,
    IBillingTransactionExecutor transactionExecutor,
    IPublisher publisher) : ICommandHandler<AdjustBalanceCommand, AdjustBalanceResponse>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IBillingTransactionExecutor _transactionExecutor = transactionExecutor;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<AdjustBalanceResponse>> Handle(AdjustBalanceCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _transactionExecutor.ExecuteAsync(async (token) =>
            {
                if (request.SourceId.HasValue)
                {
                    var duplicate = await _uow.Transactions.ExistsBySourceAsync(
                        request.Source, request.SourceId.Value, token);
                    if (duplicate)
                        return Result.Fail<AdjustBalanceResponse>(new DuplicateTransactionError(request.SourceId));
                }

                var balance = await _uow.Balances.GetByIdAsync(request.UserId, token);
                if (balance == null)
                {
                    balance = Balance.Create(request.UserId);
                    await _uow.Balances.CreateAsync(balance, token);
                }

                var result = request.Type switch
                {
                    TransactionType.Deposit => balance.Deposit(request.Amount),
                    TransactionType.Withdrawal => balance.Withdraw(request.Amount),
                    _ => Result.Fail(new Error("Неизвестный тип транзакции").WithMetadata("ErrorCode", "400"))
                };

                if (result.IsFailed)
                    return result.ToResult<AdjustBalanceResponse>();

                var transaction = request.Type switch
                {
                    TransactionType.Deposit => Transaction.CreateDeposit(request.UserId, request.Amount, request.Source, request.SourceId, request.Comment),
                    TransactionType.Withdrawal => Transaction.CreateWithdrawal(request.UserId, request.Amount, request.Source, request.SourceId, request.Comment),
                    _ => null
                };

                if (transaction == null)
                    return Result.Fail<AdjustBalanceResponse>(new Error("Не удалось создать транзакцию"));

                transaction.BalanceAfter = balance.CurrentBalance;

                await _uow.Balances.UpdateAsync(balance, token);
                await _uow.Transactions.CreateAsync(transaction, token);

                await _uow.SaveChangesAsync(token);
                
                await _publisher.Publish(new BalanceChangedEvent(balance.UserId), token);
                await _publisher.Publish(new TransactionChangedEvent(transaction.TransactionId, balance.UserId), token);

                return Result.Ok(new AdjustBalanceResponse(
                    balance.UserId,
                    balance.CurrentBalance,
                    transaction.TransactionId,
                    transaction.Amount,
                    transaction.Type.ToString()));
            }, cancellationToken);
        }
        catch (Exception ex) when (ex.IsUniqueConstraintViolation("UX_Transactions_Source_SourceId_NotNull"))
        {
            return Result.Fail<AdjustBalanceResponse>(new DuplicateTransactionError());
        }
    }
}
