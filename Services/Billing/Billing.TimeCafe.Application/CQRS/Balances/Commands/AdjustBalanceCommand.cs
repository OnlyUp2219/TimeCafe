namespace Billing.TimeCafe.Application.CQRS.Balances.Commands;

public record AdjustBalanceCommand(
    string UserId,
    decimal Amount,
    TransactionType Type,
    TransactionSource Source,
    string? SourceId = null,
    string? Comment = null) : IRequest<AdjustBalanceResult>;

public record AdjustBalanceResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Balance? Balance = null,
    Transaction? Transaction = null) : ICqrsResultV2
{
    public static AdjustBalanceResult BalanceNotFound() =>
        new(false, Code: "BalanceNotFound", Message: "Баланс не найден", StatusCode: 404);

    public static AdjustBalanceResult InsufficientFunds(decimal required, decimal available) =>
        new(false, Code: "InsufficientFunds",
            Message: $"Недостаточно средств. Требуется: {required:F2}₽, Доступно: {available:F2}₽",
            StatusCode: 400);

    public static AdjustBalanceResult DuplicateTransaction() =>
        new(false, Code: "DuplicateTransaction", Message: "Транзакция уже существует", StatusCode: 409);

    public static AdjustBalanceResult AdjustSuccess(Balance balance, Transaction transaction) =>
        new(true, Message: "Баланс обновлён", Balance: balance, Transaction: transaction);
}

public class AdjustBalanceCommandValidator : AbstractValidator<AdjustBalanceCommand>
{
    public AdjustBalanceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Пользователь не найден");

        RuleFor(x => x.SourceId)
            .Must(x => string.IsNullOrEmpty(x) || (Guid.TryParse(x, out var guid) && guid != Guid.Empty))
            .WithMessage("Некорректный SourceId")
            .When(x => !string.IsNullOrEmpty(x.SourceId));

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Сумма должна быть больше нуля");

        RuleFor(x => x.Comment)
            .MaximumLength(500).WithMessage("Комментарий не может превышать 500 символов");
    }
}

public class AdjustBalanceCommandHandler(
    IBalanceRepository balanceRepository,
    ITransactionRepository transactionRepository) : IRequestHandler<AdjustBalanceCommand, AdjustBalanceResult>
{
    private readonly IBalanceRepository _balanceRepository = balanceRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async Task<AdjustBalanceResult> Handle(AdjustBalanceCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(request.UserId);
        var sourceId = !string.IsNullOrEmpty(request.SourceId) ? Guid.Parse(request.SourceId) : (Guid?)null;

        if (sourceId.HasValue)
        {
            var duplicate = await _transactionRepository.ExistsBySourceAsync(
                request.Source, sourceId.Value, cancellationToken);
            if (duplicate)
                return AdjustBalanceResult.DuplicateTransaction();
        }

        var balance = await _balanceRepository.GetByUserIdAsync(userId, cancellationToken);
        if (balance == null)
        {
            balance = new Balance(userId);
            balance = await _balanceRepository.CreateAsync(balance, cancellationToken);
        }

        if (request.Type == TransactionType.Withdrawal && balance.CurrentBalance < request.Amount)
            return AdjustBalanceResult.InsufficientFunds(request.Amount, balance.CurrentBalance);

        if (request.Type == TransactionType.Deposit)
        {
            balance.CurrentBalance += request.Amount;
            balance.TotalDeposited += request.Amount;
        }
        else
        {
            balance.CurrentBalance -= request.Amount;
            balance.TotalSpent += request.Amount;
        }

        balance.LastUpdated = DateTimeOffset.UtcNow;

        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            UserId = userId,
            Amount = request.Amount,
            Type = request.Type,
            Source = request.Source,
            SourceId = sourceId,
            Status = TransactionStatus.Completed,
            Comment = request.Comment,
            CreatedAt = DateTimeOffset.UtcNow,
            BalanceAfter = balance.CurrentBalance
        };

        await _balanceRepository.UpdateAsync(balance, cancellationToken);
        await _transactionRepository.CreateAsync(transaction, cancellationToken);

        return AdjustBalanceResult.AdjustSuccess(balance, transaction);
    }
}
