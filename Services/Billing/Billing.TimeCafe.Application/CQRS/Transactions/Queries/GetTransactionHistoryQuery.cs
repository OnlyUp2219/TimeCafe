namespace Billing.TimeCafe.Application.CQRS.Transactions.Queries;

public sealed record GetTransactionHistoryQuery(Guid UserId, int Page, int PageSize) : IQuery<PagedResponse<TransactionDto>>;

public sealed class GetTransactionHistoryQueryHandler(IUnitOfWork uow) : IQueryHandler<GetTransactionHistoryQuery, PagedResponse<TransactionDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<PagedResponse<TransactionDto>>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken = default)
    {
        var totalCount = await _uow.Transactions.GetTotalCountByUserIdAsync(request.UserId, cancellationToken);

        var transactions = await _uow.Transactions.GetByUserIdAsync(
            request.UserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = transactions.ConvertAll(t => new TransactionDto(
            t.TransactionId,
            t.UserId,
            t.Amount,
            (int)t.Type,
            (int)t.Source,
            t.SourceId,
            (int)t.Status,
            t.Comment,
            t.CreatedAt,
            t.BalanceAfter));

        var totalPages = (totalCount + request.PageSize - 1) / request.PageSize;

        return Result.Ok(new PagedResponse<TransactionDto>(
            dtos,
            new PageMetadata(request.Page, request.PageSize, totalCount, totalPages)));
    }
}
