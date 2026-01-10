namespace Billing.TimeCafe.Application.CQRS.Transactions.Queries;

public record GetTransactionByIdQuery(string TransactionId) : IRequest<GetTransactionByIdResult>;

public record GetTransactionByIdResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    TransactionDetailDto? Transaction = null) : ICqrsResultV2
{
    public static GetTransactionByIdResult TransactionNotFound() =>
        new(false, Code: "TransactionNotFound", Message: "Транзакция не найдена", StatusCode: 404);

    public static GetTransactionByIdResult GetSuccess(TransactionDetailDto transaction) =>
        new(true, Message: "Транзакция получена", Transaction: transaction);
}
public class GetTransactionByIdQueryValidator : AbstractValidator<GetTransactionByIdQuery>
{
    public GetTransactionByIdQueryValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("Транзакция не найдена")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Транзакция не найдена");
    }
}

public class GetTransactionByIdQueryHandler(ITransactionRepository repository) : IRequestHandler<GetTransactionByIdQuery, GetTransactionByIdResult>
{
    private readonly ITransactionRepository _repository = repository;

    public async Task<GetTransactionByIdResult> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transactionId = Guid.Parse(request.TransactionId);
        var transaction = await _repository.GetByIdAsync(transactionId, cancellationToken);
        if (transaction == null)
            return GetTransactionByIdResult.TransactionNotFound();

        var dto = new TransactionDetailDto(
            transaction.TransactionId,
            transaction.UserId,
            transaction.Amount,
            (int)transaction.Type,
            (int)transaction.Source,
            transaction.SourceId,
            (int)transaction.Status,
            transaction.Comment,
            transaction.CreatedAt,
            transaction.BalanceAfter);

        return GetTransactionByIdResult.GetSuccess(dto);
    }
}
