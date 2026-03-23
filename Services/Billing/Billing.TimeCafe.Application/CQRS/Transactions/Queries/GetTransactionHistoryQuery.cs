namespace Billing.TimeCafe.Application.CQRS.Transactions.Queries;

public record GetTransactionHistoryQuery(Guid UserId, int Page = 1, int PageSize = 10) : IRequest<GetTransactionHistoryResult>;

public record GetTransactionHistoryResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    List<TransactionDto>? Transactions = null,
    int? TotalCount = null,
    int? TotalPages = null) : ICqrsResultV2
{
    public static GetTransactionHistoryResult UserNotFound() =>
        new(false, Code: "UserNotFound", Message: "Пользователь не найден", StatusCode: 404);

    public static GetTransactionHistoryResult GetSuccess(List<TransactionDto> transactions, int totalCount, int pageSize) =>
        new(true, Message: "История транзакций получена",
            Transactions: transactions,
            TotalCount: totalCount,
            TotalPages: (totalCount + pageSize - 1) / pageSize);
}

public class GetTransactionHistoryQueryValidator : AbstractValidator<GetTransactionHistoryQuery>
{
    public GetTransactionHistoryQueryValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");

        RuleFor(x => x.Page).ValidPageNumber();

        RuleFor(x => x.PageSize).ValidPageSize();
    }
}

public class GetTransactionHistoryQueryHandler(ITransactionRepository repository) : IRequestHandler<GetTransactionHistoryQuery, GetTransactionHistoryResult>
{
    private readonly ITransactionRepository _repository = repository;

    public async Task<GetTransactionHistoryResult> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        var totalCount = await _repository.GetTotalCountByUserIdAsync(request.UserId, cancellationToken);

        var transactions = await _repository.GetByUserIdAsync(
            request.UserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        if (transactions == null || transactions.Count == 0)
        {
            return GetTransactionHistoryResult.GetSuccess([], totalCount, request.PageSize);
        }

        var dtos = transactions.ConvertAll(t => new TransactionDto(
            t.TransactionId,
            t.UserId,
            t.Amount,
            (int)t.Type,
            (int)t.Source,
            (int)t.Status,
            t.Comment,
            t.CreatedAt,
            t.BalanceAfter));

        return GetTransactionHistoryResult.GetSuccess(dtos, totalCount, request.PageSize);
    }
}
