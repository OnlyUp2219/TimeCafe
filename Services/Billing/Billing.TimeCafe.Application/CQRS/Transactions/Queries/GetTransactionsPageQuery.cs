namespace Billing.TimeCafe.Application.CQRS.Transactions.Queries;

public record GetTransactionsPageQuery(int Page, int PageSize, Guid? UserId) : IRequest<GetTransactionsPageResult>;

public record GetTransactionsPageResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    List<TransactionDto>? Transactions = null,
    int? TotalCount = null,
    int? TotalPages = null) : ICqrsResult
{
    public static GetTransactionsPageResult GetSuccess(List<TransactionDto> transactions, int totalCount, int pageSize) =>
        new(true, Message: "Транзакции получены",
            Transactions: transactions,
            TotalCount: totalCount,
            TotalPages: (totalCount + pageSize - 1) / pageSize);
}

public class GetTransactionsPageQueryValidator : AbstractValidator<GetTransactionsPageQuery>
{
    public GetTransactionsPageQueryValidator()
    {
        RuleFor(x => x.Page).ValidPageNumber();
        RuleFor(x => x.PageSize).ValidPageSize();
    }
}

public class GetTransactionsPageQueryHandler(ITransactionRepository repository)
    : IRequestHandler<GetTransactionsPageQuery, GetTransactionsPageResult>
{
    public async Task<GetTransactionsPageResult> Handle(GetTransactionsPageQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetPageAsync(request.Page, request.PageSize, request.UserId, cancellationToken);

        var dtos = items.ConvertAll(t => new TransactionDto(
            t.TransactionId,
            t.UserId,
            t.Amount,
            (int)t.Type,
            (int)t.Source,
            (int)t.Status,
            t.Comment,
            t.CreatedAt,
            t.BalanceAfter));

        return GetTransactionsPageResult.GetSuccess(dtos, totalCount, request.PageSize);
    }
}
